using AdventureWorks.Application.Interfaces.Services.Address;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Address;

/// <summary>
/// The controller that coordinates retrieving Address information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/address", Name = "ReadAddressControllerV1")]
public sealed class ReadAddressController : ControllerBase
{
    private readonly ILogger<ReadAddressController> _logger;
    private readonly IReadAddressService _readAddressService;

    /// <summary>
    /// The controller that coordinates retrieving Address information.
    /// </summary>
    /// <remarks></remarks>
    public ReadAddressController( 
        ILogger<ReadAddressController> logger,
        IReadAddressService _readAddressService)
    {
        _logger = logger;
        this._readAddressService = _readAddressService;
    }

    /// <summary>
    /// Retrieve an address using its unique identifier
    /// </summary>
    /// <param name="addressId">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{addressId}")]
    [Produces(typeof(AddressModel))]
    public async Task<IActionResult> GetByIdAsync(int addressId)
    {
        if (addressId <= 0)
        {
            return BadRequest("A valid address id must be specified.");
        }

        var address = await _readAddressService.GetByIdAsync(addressId);
        
        if (address == null)
        {
            return NotFound("Unable to locate Address.");
        }

        return Ok(address);
    }
}
