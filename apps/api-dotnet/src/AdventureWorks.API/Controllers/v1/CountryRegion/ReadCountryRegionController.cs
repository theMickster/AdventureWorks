using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.CountryRegion;

/// <summary>
/// The controller that coordinates retrieving Country Region information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Country Region")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/countries", Name = "ReadCountryRegionControllerV1")]
public  class ReadCountryRegionController : ControllerBase
{
    private readonly ILogger<ReadCountryRegionController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving Country Region information.
    /// </summary>
    public ReadCountryRegionController(
        ILogger<ReadCountryRegionController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a country region using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id}", Name = "GetCountryRegionById")]
    [Produces(typeof(CountryRegionModel))]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        if ( string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("A valid country region id must be specified.");
        }

        var model = await _mediator.Send(new ReadCountryRegionQuery { Code = id });

        return model is null ? NotFound("Unable to locate the country region.") : Ok(model);

    }

    /// <summary>
    /// Retrieve the complete country region list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetCountryRegions")]
    [Produces(typeof(CountryRegionModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _mediator.Send(new ReadCountryRegionListQuery());

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records the country region list.");
        }

        return Ok(model);

    }
}