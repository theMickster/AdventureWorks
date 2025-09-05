using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// Controller for retrieving employee pay history.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees/{employeeId:int}/pay-history")]
[Authorize]
public sealed class ReadEmployeePayHistoryController : ControllerBase
{
    private readonly ILogger<ReadEmployeePayHistoryController> _logger;
    private readonly IMediator _mediator;

    public ReadEmployeePayHistoryController(
        ILogger<ReadEmployeePayHistoryController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the complete pay history for an employee, ordered by rate change date descending.
    /// </summary>
    /// <param name="employeeId">The employee's business entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ordered list of pay history records.</returns>
    [HttpGet(Name = "GetEmployeePayHistory")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeePayHistoryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayHistoryAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
        {
            return BadRequest("The employee identifier must be a positive integer.");
        }

        _logger.LogInformation("Retrieving pay history for employee {EmployeeId}.", employeeId);

        var query = new ReadEmployeePayHistoryQuery { BusinessEntityId = employeeId };
        var history = await _mediator.Send(query, cancellationToken);
        return Ok(history);
    }
}
