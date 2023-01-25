using AdventureWorks.Application.Interfaces.Services.CountryRegion;
using AdventureWorks.Domain.Models;
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
    private readonly IReadCountryRegionService _readCountryRegionService;

    /// <summary>
    /// The controller that coordinates retrieving Country Region information.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="readCountryRegionService"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReadCountryRegionController(
        ILogger<ReadCountryRegionController> logger,
        IReadCountryRegionService readCountryRegionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readCountryRegionService = readCountryRegionService ?? throw new ArgumentNullException(nameof(readCountryRegionService));
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

        var model = await _readCountryRegionService.GetByIdAsync(id).ConfigureAwait(false);

        if (model == null)
        {
            return NotFound("Unable to locate the country region.");
        }

        return Ok(model);

    }

    /// <summary>
    /// Retrieve the complete country region list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetCountryRegions")]
    [Produces(typeof(CountryRegionModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readCountryRegionService.GetListAsync().ConfigureAwait(false);

        if (!model.Any())
        {
            return NotFound("Unable to locate records the country region list.");
        }

        return Ok(model);

    }
}