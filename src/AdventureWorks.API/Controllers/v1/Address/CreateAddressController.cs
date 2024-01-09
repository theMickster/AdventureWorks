using AdventureWorks.Application.Interfaces.Services.Address;
using AdventureWorks.Domain.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Address;

/// <summary>
/// The controller that coordinates creating Address information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addresses", Name = "CreateAddressControllerV1")]
public sealed class CreateAddressController : ControllerBase
{
    private readonly ILogger<CreateAddressController> _logger;
    private readonly ICreateAddressService _createAddressService;

    public CreateAddressController(
        ILogger<CreateAddressController> logger,
        ICreateAddressService createAddressService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _createAddressService = createAddressService ?? throw  new ArgumentNullException(nameof(createAddressService));
    }

    /// <summary>
    /// Creates a new address
    /// </summary>
    /// <param name="inputModel">the address to create</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(typeof(AddressModel))]
    public async Task<IActionResult> PostAsync([FromBody] AddressCreateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The address input model cannot be null.");
        }
            
        var (addressModel, errors)  = await _createAddressService.CreateAsync(inputModel).ConfigureAwait(false);
            
        if (errors.Any())
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }
        
        return CreatedAtRoute("GetAddressById", new { addressId = addressModel.Id }, addressModel);
            
    }
}