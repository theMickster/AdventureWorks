using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.UnitMeasures;

/// <summary>
/// The controller that coordinates retrieving unit measure information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Production")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/unit-measures", Name = "ReadUnitMeasureControllerV1")]
public sealed class ReadUnitMeasureController : ControllerBase
{
    private readonly ILogger<ReadUnitMeasureController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving unit measure information.
    /// </summary>
    /// <remarks></remarks>
    public ReadUnitMeasureController(
        ILogger<ReadUnitMeasureController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a unit measure using its code
    /// </summary>
    /// <param name="code">the unit measure code</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{code}", Name = "GetUnitMeasureByCode")]
    [Produces(typeof(UnitMeasureModel))]
    public async Task<IActionResult> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("A valid unit measure code must be specified.");
        }

        var model = await _mediator.Send(new ReadUnitMeasureQuery { Code = code }, cancellationToken);

        return model is null ? NotFound("Unable to locate the unit measure.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete unit measure list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetUnitMeasures")]
    [Produces(typeof(List<UnitMeasureModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadUnitMeasureListQuery(), cancellationToken);

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the unit measure list.");
        }

        return Ok(model);
    }
}
