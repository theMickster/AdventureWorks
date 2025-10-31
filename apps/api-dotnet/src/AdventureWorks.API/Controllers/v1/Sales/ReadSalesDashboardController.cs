using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Sales;

/// <summary>
/// The controller that coordinates retrieving sales dashboard aggregate KPIs.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Sales")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/sales", Name = "ReadSalesDashboardControllerV1")]
public sealed class ReadSalesDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReadSalesDashboardController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves pre-aggregated sales dashboard KPIs for the current dataset.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>200 with <see cref="SalesDashboardModel"/> containing overall KPIs, top performers, territory breakdown, and 24-month sales trend</returns>
    [HttpGet("dashboard", Name = "GetSalesDashboard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesDashboardModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var model = await _mediator.Send(new ReadSalesDashboardQuery(), cancellationToken);
        return Ok(model);
    }
}
