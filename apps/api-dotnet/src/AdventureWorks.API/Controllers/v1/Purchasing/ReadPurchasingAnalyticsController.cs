using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Models.Features.Purchasing;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Purchasing;

/// <summary>
/// The controller that coordinates retrieving aggregate purchasing analytics.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "PurchasingAnalytics")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/purchasing", Name = "ReadPurchasingAnalyticsControllerV1")]
public sealed class ReadPurchasingAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReadPurchasingAnalyticsController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves aggregate purchasing analytics for the current dataset.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>200 with <see cref="PurchasingAnalyticsModel"/> containing the Pareto vendor-spend distribution (every vendor, ordered by spend descending, with a running cumulative percentage) and the four-status purchase order pipeline summary</returns>
    [HttpGet("analytics", Name = "GetPurchasingAnalytics")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PurchasingAnalyticsModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAnalyticsAsync(CancellationToken cancellationToken)
    {
        var model = await _mediator.Send(new ReadPurchasingAnalyticsQuery(), cancellationToken);
        return Ok(model);
    }
}
