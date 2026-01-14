using EmployeeApplication.DataBaseContext.Context;
using EmployeeApplication.Model.DTOs.EmployeeDTOs;
using EmployeeApplication.Model.DTOs.ResponseDTOS;
using EmployeeApplication.Model.EmployeeMapping;
using EmployeeApplication.Model.Models;
using EmployeeApplication.Repository.Repository.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApplication.Repository.Repository.Implementations
{
    public class EmployeeApplicationImplementation : IEmployeeApplicationService
    {
        private readonly EmployeeApplicationDbContext _employeeApplicationDbContext;

        public EmployeeApplicationImplementation(EmployeeApplicationDbContext employeeApplicationDbContext)
        {
            this._employeeApplicationDbContext = employeeApplicationDbContext;
        }

        public async Task<ResponseDto> CreateEmployeeAsync(AddEmployeeDto addEmployeeDto)
        {
            if (addEmployeeDto is null)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee data provided!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            /* Check for duplicate entry */
            var employeeEmail = await this._employeeApplicationDbContext.Employees.FirstOrDefaultAsync(employee => employee.FirstName == addEmployeeDto.FirstName);
            if (employeeEmail is not null)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "Employee with this email already exists!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            EmployeeDto employeeDto = new()
            {
                Id = new Guid(),
                FirstName = addEmployeeDto.FirstName,
                LastName = addEmployeeDto.LastName,
            };

            var employee = employeeDto.ConvertEmployeeDtoToEmployeeExtension();

            await this._employeeApplicationDbContext.Employees.AddAsync(employee);
            await this._employeeApplicationDbContext.SaveChangesAsync();
            var addedEmployeeDto = employee.ConvertEmployeeToEmployeeDtoExtension();

            return new ResponseDto()
            {
                Result = addedEmployeeDto,
                IsSuccess = true,
                Message = "Employee created successfully!",
            };
        }

        public async Task<ResponseDto> DeleteEmployeeAsync(Guid id)
        {
            if (Guid.Empty == id)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee id provided!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            var employee = await this._employeeApplicationDbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id);

            if (employee is null)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee found with this id!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            this._employeeApplicationDbContext.Remove(employee);
            await this._employeeApplicationDbContext.SaveChangesAsync();

            EmployeeDto employeeDto = new()
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
            };

            return new ResponseDto()
            {
                Result = employeeDto,
                IsSuccess = true,
                Message = "Employee deleted successfully!",
            };
        }

        public async Task<ResponseDto> GetEmployeeByIdAsync(Guid id)
        {
            var fetchedEmployee = await this._employeeApplicationDbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id);

            if (fetchedEmployee is null)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee found with this id!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            var employeeDto = fetchedEmployee.ConvertEmployeeToEmployeeDtoExtension();

            return new ResponseDto()
            {
                Result = employeeDto,
                IsSuccess = true,
                Message = "Success",
            };
        }

        public async Task<ResponseDto> GetEmployeesAsync()
        {
            var employees = await this._employeeApplicationDbContext.Employees.ToListAsync();

            IList<EmployeeDto> employeesDto = [];

            if (employees is null || !employees.Any())
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employees found!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            foreach (var employee in employees)
            {
                var employeeDto = employee.ConvertEmployeeToEmployeeDtoExtension();
                employeesDto.Add(employeeDto);
            }
            return new ResponseDto()
            {
                Result = employeesDto,
                IsSuccess = true,
                Message = "Success",
            };
        }

        public async Task<ResponseDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            if (id == Guid.Empty)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee id provided!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            var employee = await this._employeeApplicationDbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id);

            if (employee is null)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee found with this id!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            if (updateEmployeeDto is null)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "No employee data is provided to update!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            if (employee.FirstName == updateEmployeeDto.FirstName && employee.LastName == updateEmployeeDto.LastName)
            {
                ApplicationError applicationError = new()
                {
                    ID = new Guid(),
                    When = DateTime.Now,
                    Message = "Duplicated employee data is trying to be updated!",
                };

                return new ResponseDto()
                {
                    Result = null,
                    IsSuccess = false,
                    Message = applicationError.Message,
                };
            }

            employee.FirstName = updateEmployeeDto.FirstName;
            employee.LastName = updateEmployeeDto.LastName;

            await this._employeeApplicationDbContext.SaveChangesAsync();

            var updatedEmployeeDto = employee.ConvertEmployeeToEmployeeDtoExtension();

            return new ResponseDto()
            {
                Result = updatedEmployeeDto,
                IsSuccess = true,
                Message = "Employee updated successfully!",
            };
        }
    }
}
