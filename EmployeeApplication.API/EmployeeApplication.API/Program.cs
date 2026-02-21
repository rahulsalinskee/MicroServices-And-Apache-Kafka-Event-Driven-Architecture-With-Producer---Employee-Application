using EmployeeApplication.API.AttackProtections.CorsIntegration;
using EmployeeApplication.API.AttackProtections.CSRFProtection;
using EmployeeApplication.API.AttackProtections.RateLimitation;
using EmployeeApplication.DataBaseContext.Context;
using EmployeeApplication.DataBaseContext.DbConfiguration;
using EmployeeApplication.Exception;
using EmployeeApplication.Log;
using EmployeeApplication.Repository.Repository.Implementations;
using EmployeeApplication.Repository.Repository.Services;
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

    builder.Services.AddScoped<IEmployeeApplicationService, EmployeeApplicationImplementation>();

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    /* --- Rate Limiting START: Configure Rate Limiting --- */
    builder.Services.AddRateLimitingServicesExtension();
    /* --- Rate Limiting END --- */

    /* --- Antiforgery START: Configure Services For CSRF Attack Prevention --- */
    builder.Services.AddAppAntiForgeryExtension();
    /* --- Antiforgery END: CSRF Attack Prevention --- */

    /* -- CORS START: Service Registration */
    builder.Services.AddCorsServicesExtension(builder.Configuration);
    /* -- CORS STOP: Service Registration */

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

    app.UseRouting();

    /* --- CORS Middleware START --- */
    /* Must be between UseRouting and UseRateLimiter/UseAuthorization */
    app.UseCorsMiddlewareExtension();
    /* --- CORS Middleware END --- */

    /* --- Antiforgery Middleware START --- */
    /* This middleware provides the token to the client via a cookie */
    app.UseAntiForgeryTokenMiddlewareExtension();
    /* --- Antiforgery Middleware END --- */

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