using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Manufacturing;

/// <summary>
/// Controller for aggregate manufacturing KPI data.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ManufacturingKpis")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/manufacturing", Name = "ReadManufacturingKpisControllerV1")]
public sealed class ReadManufacturingKpisController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Retrieves aggregate manufacturing KPIs across all production work orders.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>200 with the aggregate manufacturing KPI model.</returns>
    [HttpGet("kpis", Name = "GetManufacturingKpis")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ManufacturingKpisModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKpisAsync(CancellationToken cancellationToken)
    {
        var model = await _mediator.Send(new ReadManufacturingKpisQuery(), cancellationToken);
        return Ok(model);
    }
}
