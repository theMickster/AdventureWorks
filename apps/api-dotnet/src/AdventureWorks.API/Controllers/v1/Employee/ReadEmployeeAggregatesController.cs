using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// The controller that returns pre-computed HR dashboard aggregate statistics.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees", Name = "ReadEmployeeAggregatesControllerV1")]
public sealed class ReadEmployeeAggregatesController : ControllerBase
{
    private readonly ILogger<ReadEmployeeAggregatesController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that returns pre-computed HR dashboard aggregate statistics.
    /// </summary>
    public ReadEmployeeAggregatesController(
        ILogger<ReadEmployeeAggregatesController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Returns HR dashboard aggregate statistics: department headcounts, tenure distribution, and pay band summary.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Aggregate statistics model</returns>
    /// <response code="200">Aggregates returned successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("aggregates", Name = "GetEmployeeAggregates")]
    [ProducesResponseType(typeof(EmployeeAggregatesModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAggregatesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ReadEmployeeAggregatesQuery(), cancellationToken);

        return Ok(result);
    }
}
