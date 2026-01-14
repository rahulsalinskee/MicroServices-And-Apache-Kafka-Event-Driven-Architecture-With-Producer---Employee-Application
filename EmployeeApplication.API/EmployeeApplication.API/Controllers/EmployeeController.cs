using EmployeeApplication.API.ServerSideValidation;
using EmployeeApplication.Model.DTOs.EmployeeDTOs;
using EmployeeApplication.Repository.Repository.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeApplicationService _employeeApplicationService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeApplicationService employeeApplicationService, ILogger<EmployeeController> logger)
        {
            this._employeeApplicationService = employeeApplicationService;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var response = await this._employeeApplicationService.GetEmployeesAsync();

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error fetching employees: {Message}", response.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById([FromRoute] Guid id)
        {
            var response = await this._employeeApplicationService.GetEmployeeByIdAsync(id: id);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error fetching employee by ID {Id}: {Message}", id, response.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("create")]
        [ModelValidation]
        public async Task<IActionResult> CreateEmployee([FromBody] AddEmployeeDto addEmployeeDto)
        {
            var response = await this._employeeApplicationService.CreateEmployeeAsync(addEmployeeDto: addEmployeeDto);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error creating employee: {Message}", response.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut("update/{id}")]
        [ModelValidation]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            var response = await this._employeeApplicationService.UpdateEmployeeAsync(id: id, updateEmployeeDto: updateEmployeeDto);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error updating employee with ID {Id}: {Message}", id, response.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
        {
            var response = await this._employeeApplicationService.DeleteEmployeeAsync(id: id);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error deleting employee with ID {Id}: {Message}", id, response.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
