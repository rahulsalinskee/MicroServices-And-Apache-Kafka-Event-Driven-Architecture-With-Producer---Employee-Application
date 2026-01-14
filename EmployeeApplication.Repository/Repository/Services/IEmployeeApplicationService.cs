using EmployeeApplication.Model.DTOs.EmployeeDTOs;
using EmployeeApplication.Model.DTOs.ResponseDTOS;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeApplication.Repository.Repository.Services
{
    public interface IEmployeeApplicationService
    {
        public Task<ResponseDto> GetEmployeesAsync();

        public Task<ResponseDto> GetEmployeeByIdAsync(Guid id);

        public Task<ResponseDto> CreateEmployeeAsync(AddEmployeeDto addEmployeeDto);

        public Task<ResponseDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto);

        public Task<ResponseDto> DeleteEmployeeAsync(Guid id);
    }
}
