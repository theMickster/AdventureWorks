using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Manufacturing;

/// <summary>
/// Controller for the manufacturing quality scorecard.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ManufacturingQualityScorecard")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/manufacturing", Name = "ReadManufacturingQualityScorecardControllerV1")]
public sealed class ReadManufacturingQualityScorecardController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Retrieves the top and bottom product quality rankings and scrap-reason breakdown.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>200 with the manufacturing quality scorecard.</returns>
    [HttpGet("quality-scorecard", Name = "GetManufacturingQualityScorecard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ManufacturingQualityScorecardModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQualityScorecardAsync(CancellationToken cancellationToken)
    {
        var model = await _mediator.Send(new ReadManufacturingQualityScorecardQuery(), cancellationToken);
        return Ok(model);
    }
}
