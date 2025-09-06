using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Locations;

/// <summary>
/// The controller that coordinates retrieving location information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Production")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/locations", Name = "ReadLocationControllerV1")]
public sealed class ReadLocationController : ControllerBase
{
    private readonly ILogger<ReadLocationController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving location information.
    /// </summary>
    /// <remarks></remarks>
    public ReadLocationController(
        ILogger<ReadLocationController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a location using its unique identifier
    /// </summary>
    /// <param name="id">the unique location identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetLocationById")]
    [Produces(typeof(LocationModel))]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0 || id > short.MaxValue)
        {
            return BadRequest("A valid location id must be specified.");
        }

        var model = await _mediator.Send(new ReadLocationQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the location.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete location list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetLocations")]
    [Produces(typeof(List<LocationModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadLocationListQuery(), cancellationToken);

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the location list.");
        }

        return Ok(model);
    }
}
