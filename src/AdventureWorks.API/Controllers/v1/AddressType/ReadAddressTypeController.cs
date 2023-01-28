using AdventureWorks.Application.Interfaces.Services.AddressType;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.AddressType;

/// <summary>
/// The controller that coordinates retrieving Address Type information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address Type")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addressTypes", Name = "ReadAddressTypeControllerV1")]
public sealed class ReadAddressTypeController : ControllerBase
{
    private readonly ILogger<ReadAddressTypeController> _logger;
    private readonly IReadAddressTypeService _readAddressTypeService;

    /// <summary>
    /// The controller that coordinates retrieving Address Type information.
    /// </summary>
    /// <remarks></remarks>
    public ReadAddressTypeController(
        ILogger<ReadAddressTypeController> logger,
        IReadAddressTypeService readAddressTypeService
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readAddressTypeService = readAddressTypeService ?? throw new ArgumentNullException(nameof(readAddressTypeService));
    }


    /// <summary>
    /// Retrieve a address type using its unique identifier
    /// </summary>
    /// <param name="id">the unique identifier</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetAddressTypeById")]
    [Produces(typeof(AddressTypeModel))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest("A valid address type id must be specified.");
        }

        var model = await _readAddressTypeService.GetByIdAsync(id).ConfigureAwait(false);

        if (model == null)
        {
            return NotFound("Unable to locate the address type.");
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve the complete address type list
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetAddressTypes")]
    [Produces(typeof(AddressTypeModel))]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readAddressTypeService.GetListAsync().ConfigureAwait(false);

        if (!model.Any())
        {
            return NotFound("Unable to locate records the address type list.");
        }

        return Ok(model);
    }
}
