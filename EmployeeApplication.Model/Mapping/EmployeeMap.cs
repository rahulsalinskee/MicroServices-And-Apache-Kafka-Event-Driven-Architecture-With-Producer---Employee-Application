using EmployeeApplication.Model.DTOs.EmployeeDTOs;
using EmployeeApplication.Model.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeApplication.Model.EmployeeMapping
{
    public static class EmployeeMap
    {
        public static Employee ConvertEmployeeDtoToEmployeeExtension(this EmployeeDto employeeDto)
        {
            return new Employee()
            {
                Id = employeeDto.Id,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName
            };
        }

        public static EmployeeDto ConvertEmployeeToEmployeeDtoExtension(this Employee employee)
        {
            return new EmployeeDto()
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName
            };
        }
    }
}
