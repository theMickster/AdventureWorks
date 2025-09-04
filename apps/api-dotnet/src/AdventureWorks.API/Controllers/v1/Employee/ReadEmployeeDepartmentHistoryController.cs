using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for retrieving employee department assignment history.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees/{employeeId:int}/department-history")]
[Authorize]
public sealed class ReadEmployeeDepartmentHistoryController : ControllerBase
{
    private readonly ILogger<ReadEmployeeDepartmentHistoryController> _logger;
    private readonly IMediator _mediator;

    public ReadEmployeeDepartmentHistoryController(
        ILogger<ReadEmployeeDepartmentHistoryController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the complete department assignment history for an employee, ordered by start date descending.
    /// </summary>
    /// <param name="employeeId">The employee's business entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ordered list of department history records.</returns>
    [HttpGet(Name = "GetEmployeeDepartmentHistory")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDepartmentHistoryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistoryAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
        {
            return BadRequest("The employee identifier must be a positive integer.");
        }

        _logger.LogInformation("Retrieving department history for employee {EmployeeId}.", employeeId);

        var query = new ReadEmployeeDepartmentHistoryQuery { BusinessEntityId = employeeId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
