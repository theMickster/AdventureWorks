using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for managing employee lifecycle operations (hire, terminate, rehire).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees", Name = "EmployeeLifecycleControllerV1")]
[Authorize]
public sealed class EmployeeLifecycleController : ControllerBase
{
    private readonly ILogger<EmployeeLifecycleController> _logger;
    private readonly IMediator _mediator;

    public EmployeeLifecycleController(
        ILogger<EmployeeLifecycleController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Hires an employee by activating them and creating initial department assignment, pay rate, and PTO balances.
    /// </summary>
    /// <param name="employeeId">The business entity ID of the employee to hire</param>
    /// <param name="inputModel">Hire details including department, pay rate, and PTO</param>
    /// <returns>The employee's business entity ID</returns>
    /// <response code="200">Employee hired successfully</response>
    /// <response code="400">Invalid input data or business rule violation (e.g., employee already active)</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{employeeId:int}/lifecycle/hire")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HireAsync(
        int employeeId,
        [FromBody] EmployeeHireModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning("HireEmployee called with null input model for EmployeeId: {EmployeeId}", employeeId);
            return BadRequest("The hire input model cannot be null.");
        }

        if (employeeId != inputModel.EmployeeId)
        {
            _logger.LogWarning(
                "Route employeeId {RouteId} does not match model EmployeeId {ModelId}",
                employeeId,
                inputModel.EmployeeId);
            return BadRequest("The employee ID in the route must match the employee ID in the request body.");
        }

        _logger.LogInformation(
            "Hiring employee {EmployeeId}: HireDate={HireDate}, DepartmentId={DepartmentId}, PayRate={PayRate}",
            employeeId,
            inputModel.HireDate,
            inputModel.DepartmentId,
            inputModel.InitialPayRate);

        try
        {
            var command = new HireEmployeeCommand
            {
                Model = inputModel,
                ModifiedDate = DateTime.UtcNow
            };

            var businessEntityId = await _mediator.Send(command);

            _logger.LogInformation(
                "Employee {EmployeeId} hired successfully on {HireDate}",
                businessEntityId,
                inputModel.HireDate);

            return Ok(new { businessEntityId, message = "Employee hired successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Employee {EmployeeId} not found during hire operation", employeeId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation during hire of employee {EmployeeId}", employeeId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Terminates an employee by deactivating them, closing their department assignment, and optionally paying out PTO.
    /// </summary>
    /// <param name="employeeId">The business entity ID of the employee to terminate</param>
    /// <param name="inputModel">Termination details including date, reason, and PTO payout preference</param>
    /// <returns>Success status</returns>
    /// <response code="200">Employee terminated successfully</response>
    /// <response code="400">Invalid input data or business rule violation (e.g., employee already terminated)</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{employeeId:int}/lifecycle/terminate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TerminateAsync(
        int employeeId,
        [FromBody] EmployeeTerminateModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning("TerminateEmployee called with null input model for EmployeeId: {EmployeeId}", employeeId);
            return BadRequest("The termination input model cannot be null.");
        }

        if (employeeId != inputModel.EmployeeId)
        {
            _logger.LogWarning(
                "Route employeeId {RouteId} does not match model EmployeeId {ModelId}",
                employeeId,
                inputModel.EmployeeId);
            return BadRequest("The employee ID in the route must match the employee ID in the request body.");
        }

        _logger.LogInformation(
            "Terminating employee {EmployeeId}: TerminationDate={TerminationDate}, Type={Type}, Reason={Reason}",
            employeeId,
            inputModel.TerminationDate,
            inputModel.TerminationType,
            inputModel.Reason);

        try
        {
            var command = new TerminateEmployeeCommand
            {
                Model = inputModel,
                ModifiedDate = DateTime.UtcNow
            };

            await _mediator.Send(command);

            _logger.LogInformation(
                "Employee {EmployeeId} terminated successfully on {TerminationDate}",
                employeeId,
                inputModel.TerminationDate);

            return Ok(new { message = "Employee terminated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Employee {EmployeeId} not found during termination operation", employeeId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation during termination of employee {EmployeeId}", employeeId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Rehires a previously terminated employee. Validates 90-day minimum period since termination.
    /// </summary>
    /// <param name="employeeId">The business entity ID of the employee to rehire</param>
    /// <param name="inputModel">Rehire details including date, department, pay rate, and seniority restoration preference</param>
    /// <returns>The employee's business entity ID</returns>
    /// <response code="200">Employee rehired successfully</response>
    /// <response code="400">Invalid input data or business rule violation (e.g., less than 90 days since termination)</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{employeeId:int}/lifecycle/rehire")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RehireAsync(
        int employeeId,
        [FromBody] EmployeeRehireModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogWarning("RehireEmployee called with null input model for EmployeeId: {EmployeeId}", employeeId);
            return BadRequest("The rehire input model cannot be null.");
        }

        if (employeeId != inputModel.EmployeeId)
        {
            _logger.LogWarning(
                "Route employeeId {RouteId} does not match model EmployeeId {ModelId}",
                employeeId,
                inputModel.EmployeeId);
            return BadRequest("The employee ID in the route must match the employee ID in the request body.");
        }

        _logger.LogInformation(
            "Rehiring employee {EmployeeId}: RehireDate={RehireDate}, DepartmentId={DepartmentId}, PayRate={PayRate}, RestoreSeniority={RestoreSeniority}",
            employeeId,
            inputModel.RehireDate,
            inputModel.DepartmentId,
            inputModel.PayRate,
            inputModel.RestoreSeniority);

        try
        {
            var command = new RehireEmployeeCommand
            {
                Model = inputModel,
                ModifiedDate = DateTime.UtcNow
            };

            var businessEntityId = await _mediator.Send(command);

            _logger.LogInformation(
                "Employee {EmployeeId} rehired successfully on {RehireDate}",
                businessEntityId,
                inputModel.RehireDate);

            return Ok(new { businessEntityId, message = "Employee rehired successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Employee {EmployeeId} not found during rehire operation", employeeId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation during rehire of employee {EmployeeId}: {Message}", employeeId, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves comprehensive lifecycle status for an employee including employment status, current assignment, pay rate, PTO balances, and termination history.
    /// </summary>
    /// <param name="employeeId">The business entity ID of the employee</param>
    /// <returns>Employee lifecycle status data</returns>
    /// <response code="200">Lifecycle status retrieved successfully</response>
    /// <response code="400">Invalid employee ID provided</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{employeeId:int}/lifecycle/status")]
    [ProducesResponseType(typeof(EmployeeLifecycleStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLifecycleStatusAsync(int employeeId)
    {
        if (employeeId <= 0)
        {
            _logger.LogWarning("Invalid employee ID provided: {EmployeeId}", employeeId);
            return BadRequest("A valid employee ID must be specified.");
        }

        _logger.LogInformation("Retrieving lifecycle status for employee {EmployeeId}", employeeId);

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = employeeId };
        var model = await _mediator.Send(query);

        if (model is null)
        {
            _logger.LogWarning("Employee {EmployeeId} not found when retrieving lifecycle status", employeeId);
            return NotFound("Unable to locate the employee.");
        }

        _logger.LogInformation(
            "Successfully retrieved lifecycle status for employee {EmployeeId}: Status={Status}",
            employeeId,
            model.EmploymentStatus);

        return Ok(model);
    }
}
