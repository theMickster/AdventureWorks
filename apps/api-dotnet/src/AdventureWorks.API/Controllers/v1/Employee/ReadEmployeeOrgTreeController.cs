using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Employee;

/// <summary>
/// The controller that returns the employee organization hierarchy tree.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Human Resources")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/employees", Name = "ReadEmployeeOrgTreeControllerV1")]
public sealed class ReadEmployeeOrgTreeController : ControllerBase
{
    private readonly ILogger<ReadEmployeeOrgTreeController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that returns the employee organization hierarchy tree.
    /// </summary>
    public ReadEmployeeOrgTreeController(
        ILogger<ReadEmployeeOrgTreeController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Returns a flat list of all active employees with parent-child org hierarchy relationships.
    /// The root node (CEO) will have a null parentEmployeeId.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Flat org tree suitable for rendering a hierarchy chart</returns>
    /// <response code="200">Org tree returned successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("org-tree", Name = "GetEmployeeOrgTree")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeOrgTreeItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrgTreeAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ReadEmployeeOrgTreeQuery(), cancellationToken);

        return Ok(result);
    }
}
