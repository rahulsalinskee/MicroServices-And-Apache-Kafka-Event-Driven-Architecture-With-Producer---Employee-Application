using EmployeeApplication.DataBaseContext.Context;
using EmployeeApplication.DataBaseContext.DbConfiguration;
using EmployeeApplication.Exception;
using EmployeeApplication.Log;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

    // Add services to the container.
    builder.Services.AddControllers();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

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