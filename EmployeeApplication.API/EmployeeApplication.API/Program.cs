using EmployeeApplication.DataBaseContext.Context;
using EmployeeApplication.DataBaseContext.DbConfiguration;
using EmployeeApplication.Exception;
using EmployeeApplication.Log;
using EmployeeApplication.Model.DTOs.ResponseDTOS;
using EmployeeApplication.Model.Models;
using EmployeeApplication.Repository.Repository.Implementations;
using EmployeeApplication.Repository.Repository.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

/* Load configuration from data layer embedded resource. 
*  This forces the API to load the config file embedded inside your library 
*/
DataBaseConfiguration.GetDataBaseConnectionString(builder: builder.Configuration);

/* Get Employee Log */
var employeeLog = LogConfiguration.GenerateEmployeeLog();

Log.Logger = employeeLog;

try
{
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger: employeeLog);

    builder.Services.AddDbContext<EmployeeApplicationDbContext>(option =>
    {
        option.UseSqlServer(builder.Configuration.GetConnectionString(name: "EmployeeApplicationDbConnectionString"));
    });

    builder.Services.AddScoped<IEmployeeApplicationService, EmployeeApplicationImplementation>();

    // Add services to the container.
    builder.Services.AddControllers();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    /* --- Rate Limiting START: Configure Rate Limiting --- */
    builder.Services.AddRateLimiter(options =>
    {
        /* 1. Define the status code for rejected requests */
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        /* 
        *  2. Define a Global Policy (Applied to all endpoints)
        *  We use "Partitioned" rate limiting to track limits separately for each IP Address.
        */
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            /* Get the User's IP. If null (e.g. localhost), use "unknown". */
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter
            (
                partitionKey: remoteIp,
                factory: partition => new FixedWindowRateLimiterOptions()
                {
                    /* Logic: Allow 100 requests per 1 minute per IP */
                    AutoReplenishment = true,
                    PermitLimit = 1,
                    Window = TimeSpan.FromMinutes(1),
                    /* Do not queue excessive requests; reject them immediately */
                    QueueLimit = 0
                }
            );
        });

        /* 
        *  3. Add the "StrictPolicy" (Integrate the specific code snippet here)
        *  You can apply this to specific controllers using [EnableRateLimiting("StrictPolicy")]
        */
        options.AddPolicy(policyName: "StrictPolicy", partitioner: context =>
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions()
            {
                PermitLimit = 5, // Only 5 creates allowed
                Window = TimeSpan.FromMinutes(1), // per minute
                QueueLimit = 0
            });
        });

        /* 
        *  4. Customize the Response to match your "ResponseDto" format
        *  This ensures your frontend gets a consistent JSON error structure.
        */
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.ContentType = "application/json";

            ApplicationError applicationError = new()
            {
                ID = Guid.NewGuid(),
                Message = "Too many requests. Please slow down and try again later.",
                When = DateTime.Now,
            };

            var responseDto = new ResponseDto()
            {
                IsSuccess = false,
                Result = null,
                Message = applicationError.Message,
                DateTimeOnFailure = applicationError.When,
            };

            await context.HttpContext.Response.WriteAsJsonAsync(responseDto, token);
        };
    });

    /* --- Rate Limiting END --- */

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint(url: "/openapi/v1.json", name: "Employee Application API");
        });
    }

    app.UseMiddleware<GlobalExceptionHandler>();

    app.UseHttpsRedirection();

    /* --- Enable Rate Limiting Middleware : START --- */
    /* Place this AFTER HttpsRedirection but BEFORE Authorization/Controllers */
    app.UseRateLimiter();
    /* --- Enable Rate Limiting Middleware : END --- */

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}