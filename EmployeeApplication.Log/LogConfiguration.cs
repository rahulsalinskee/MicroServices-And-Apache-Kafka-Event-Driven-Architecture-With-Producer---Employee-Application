using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace EmployeeApplication.Log
{
    public static class LogConfiguration
    {
        private const string LOG_LIBRARY_NAME = "EmployeeApplication.Log";
        private const string LOG_DIRECTORY_NAME = "Logs";
        private const string EMPLOYEE_LOG_DIRECTORY = "EmployeeLog";
        private const string EMPLOYEE_LOG_FILE_NAME = "EmployeeLog-.txt";
        private const string EMPLOYEE_PROJECT_NAME = "EmployeeApplication.API";

        public static ILogger GenerateEmployeeLog()
        {
            var logFilePathForProductApi = GetFilePath(logApiDirName: EMPLOYEE_LOG_DIRECTORY, logFileName: EMPLOYEE_LOG_FILE_NAME);
            return CreateLog(logPath: logFilePathForProductApi, projectName: EMPLOYEE_PROJECT_NAME);
        }

        private static string GetFilePath(string logApiDirName, string logFileName)
        {
            var solutionRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
            var logDirectory = Path.Combine(solutionRoot, LOG_LIBRARY_NAME, LOG_DIRECTORY_NAME, logApiDirName);
            Directory.CreateDirectory(logDirectory);
            var logPath = Path.Combine(logDirectory, logFileName);
            return logPath;
        }

        private static Logger CreateLog(string logPath, object projectName)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty(name: "ApplicationName", value: projectName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File
                    (
                        path: logPath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ApplicationName}] {Message:lj}{NewLine}{Exception} \n",
                        retainedFileCountLimit: 7
                    ).CreateLogger();
        }
    }
}
