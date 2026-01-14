using EmployeeApplication.DataBaseContext.DbConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmployeeApplication.DataBaseContext.Context
{
    internal class EmployeeApplicationDbContextFactory : IDesignTimeDbContextFactory<EmployeeApplicationDbContext>
    {
        private const string EMPLOYEE_APPLICATION_CONNECTION_STRING = "EmployeeApplicationDbConnectionString";

        public EmployeeApplicationDbContext CreateDbContext(string[] args)
        {
            /* 1. Create a generic builder */
            var builder = new ConfigurationBuilder();

            /* 2. Load the configuration using the existing Embedded Resource helper
            *  This avoids all file path issues!
            */
            var configuration = DataBaseConfiguration.GetDataBaseConnectionString(builder);

            var optionsBuilder = new DbContextOptionsBuilder<EmployeeApplicationDbContext>();

            /* 3. Read Connection String */
            var connectionString = configuration.GetConnectionString(EMPLOYEE_APPLICATION_CONNECTION_STRING);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{EMPLOYEE_APPLICATION_CONNECTION_STRING}' not found.");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new EmployeeApplicationDbContext(optionsBuilder.Options);
        }
    }
}
