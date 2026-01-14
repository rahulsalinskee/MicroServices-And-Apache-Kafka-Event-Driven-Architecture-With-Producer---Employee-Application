using EmployeeApplication.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApplication.DataBaseContext.Context
{
    public class EmployeeApplicationDbContext : DbContext
    {
        public EmployeeApplicationDbContext(DbContextOptions<EmployeeApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
