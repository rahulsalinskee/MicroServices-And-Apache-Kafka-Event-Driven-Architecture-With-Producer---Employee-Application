using Confluent.Kafka;
using EmployeeApplication.DataBaseContext.Context;
using EmployeeApplication.Model.DTOs.EmployeeDTOs;
using EmployeeApplication.Model.DTOs.ResponseDTOS;
using EmployeeApplication.Model.EmployeeMapping;
using EmployeeApplication.Model.Models;
using EmployeeApplication.Repository.Repository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.Json;

namespace EmployeeApplication.Repository.Repository.Implementations
{
    public class EmployeeApplicationImplementation : IEmployeeApplicationService
    {
        private const string ADD_EMPLOYEE_TOPIC = "add-employee-topic";
        private const string UPDATE_EMPLOYEE_TOPIC = "update-employee-topic";
        private const string DELETE_EMPLOYEE_TOPIC = "delete-employee-topic";
        private const string RESOURCE_NAME = "EmployeeApplication.Repository.appsettings.json";

        private readonly EmployeeApplicationDbContext _employeeApplicationDbContext;
        private readonly IConfiguration _configuration;

        public EmployeeApplicationImplementation(EmployeeApplicationDbContext employeeApplicationDbContext, IConfiguration configuration)
        {
            this._employeeApplicationDbContext = employeeApplicationDbContext;
            this._configuration = configuration;
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
                    DateTimeOnFailure = applicationError.When,
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
                    DateTimeOnFailure = applicationError.When,
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

            var message = GetMessageConfiguration(messageKey: addedEmployeeDto.Id.ToString(), employeeObject: addedEmployeeDto);
            await ProducerConfigurationAsync(topicName: ADD_EMPLOYEE_TOPIC, message: message);

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
                    DateTimeOnFailure = applicationError.When,
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
                    DateTimeOnFailure = applicationError.When,
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

            var message = GetMessageConfiguration(messageKey: employeeDto.Id.ToString(), employeeObject: employeeDto);
            await ProducerConfigurationAsync(topicName: DELETE_EMPLOYEE_TOPIC, message: message);

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
                    DateTimeOnFailure = applicationError.When,
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
                    DateTimeOnFailure = applicationError.When,
                };
            }

            foreach (var employee in employees)
            {
                var employeeDto = employee.ConvertEmployeeToEmployeeDtoExtension();
                employeesDto.Add(employeeDto);
            }
            return new ResponseDto()
            {
                IsSuccess = true,
                Message = "Success",
                Result = employeesDto,
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
                    DateTimeOnFailure = applicationError.When,
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
                    DateTimeOnFailure = applicationError.When,
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
                    DateTimeOnFailure = applicationError.When,
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
                    DateTimeOnFailure = applicationError.When,
                };
            }

            employee.FirstName = updateEmployeeDto.FirstName;
            employee.LastName = updateEmployeeDto.LastName;

            await this._employeeApplicationDbContext.SaveChangesAsync();

            var updatedEmployeeDto = employee.ConvertEmployeeToEmployeeDtoExtension();

            var message = GetMessageConfiguration(messageKey: updatedEmployeeDto.Id.ToString(), employeeObject: updatedEmployeeDto);
            await ProducerConfigurationAsync(topicName: UPDATE_EMPLOYEE_TOPIC, message: message);

            return new ResponseDto()
            {
                Result = updatedEmployeeDto,
                IsSuccess = true,
                Message = "Employee updated successfully!",
            };
        }

        private static Message<string, string> GetMessageConfiguration(string messageKey, object employeeObject)
        {
            var message = new Message<string, string>()
            {
                Key = messageKey,
                Value = JsonSerializer.Serialize(employeeObject),
            };

            return message;
        }

        private async Task ProducerConfigurationAsync(string topicName, Message<string, string> message)
        {
            string bootstrapServerValue = GetKafkaConfigurationValue();
            ProducerConfig producerConfiguration = new()
            {
                //BootstrapServers = bootstrapServerValue,
                BootstrapServers = "localhost:9094",
                Acks = Acks.All,
            };

            IProducer<string, string> producerBuilder = new ProducerBuilder<string, string>(producerConfiguration).Build();

            await producerBuilder.ProduceAsync(topic: topicName, message: message);
            producerBuilder.Dispose();
        }

        private string GetKafkaConfigurationValue()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var manifestResourceStream = assembly.GetManifestResourceStream(name: RESOURCE_NAME);

            if (manifestResourceStream is not null)
            {
                //builder.AddJsonStream(stream: manifestResourceStream);
            }

            return _configuration.GetSection(key: "Kafka:BootstrapServers").Value ?? string.Empty;
        }
    }
}
