using AdventureWorks.Application.Interfaces.Services.CountryRegion;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.CountryRegion;

/// <summary>
/// The controller that coordinates retrieving Country Region information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "CountryRegion")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/countryRegion", Name = "ReadCountryRegionControllerV1")]
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
}