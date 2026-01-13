using EmployeeApplication.Exception;
using EmployeeApplication.Log;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

/* Get Employee Log */
var employeeLog = LogConfiguration.GenerateEmployeeLog();

Log.Logger = employeeLog;

try
{
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger: employeeLog);

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