using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeApplication.Model.DTOs.EmployeeDTOs
{
    public class UpdateEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
