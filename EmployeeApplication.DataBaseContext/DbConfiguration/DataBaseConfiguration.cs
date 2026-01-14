using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace EmployeeApplication.DataBaseContext.DbConfiguration
{
    public static class DataBaseConfiguration
    {
        private const string RESOURCE_NAME = "EmployeeApplication.DataBaseContext.appsettings.json";

        public static IConfiguration GetDataBaseConnectionString(IConfigurationBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            /* Helpful Debugging: If this is null, the name above is wrong. */
            using var manifestResourceStream = assembly.GetManifestResourceStream(name: RESOURCE_NAME);

            if (manifestResourceStream is not null)
            {
                builder.AddJsonStream(stream: manifestResourceStream);
            }
            else
            {
                /* Throw specific error to avoid generic crashes later */
                throw new InvalidOperationException($"Could not find Embedded Resource: {RESOURCE_NAME}");
            }
            return builder.Build();
        }
    }
}
