using EmployeeApplication.API.ServerSideValidation;
using EmployeeApplication.Model.DTOs.EmployeeDTOs;
using EmployeeApplication.Repository.Repository.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
                this._logger.LogError("Error fetching employees: {Message} at {When}", response.Message, response.DateTimeOnFailure);
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
                this._logger.LogError("Error fetching employee by ID {Id}: {Message} {When}", id, response.Message, response.DateTimeOnFailure);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("create")]
        [ModelValidation]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("StrictPolicy")]
        public async Task<IActionResult> CreateEmployee([FromBody] AddEmployeeDto addEmployeeDto)
        {
            var response = await this._employeeApplicationService.CreateEmployeeAsync(addEmployeeDto: addEmployeeDto);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error creating employee: {Message} {When}", response.Message, response.DateTimeOnFailure);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut("update/{id}")]
        [ModelValidation]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("StrictPolicy")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            var response = await this._employeeApplicationService.UpdateEmployeeAsync(id: id, updateEmployeeDto: updateEmployeeDto);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error updating employee with ID {Id}: {Message} {When}", id, response.Message, response.DateTimeOnFailure);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpDelete("delete/{id}")]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("StrictPolicy")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
        {
            var response = await this._employeeApplicationService.DeleteEmployeeAsync(id: id);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                this._logger.LogError("Error deleting employee with ID {Id}: {Message} {When}", id, response.Message, response.DateTimeOnFailure);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
