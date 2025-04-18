using AdventureWorks.Application.Features.AddressManagement.Contracts;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Address;

/// <summary>
/// The controller that coordinates updating Address information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Address")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/addresses", Name = "UpdateAddressControllerV1")]
public sealed class UpdateAddressController : ControllerBase
{
    private readonly ILogger<UpdateAddressController> _logger;
    private readonly IUpdateAddressService _updateAddressService;

    public UpdateAddressController(
        ILogger<UpdateAddressController> logger,
        IUpdateAddressService updateAddressService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _updateAddressService = updateAddressService ?? throw new ArgumentNullException(nameof(updateAddressService));
    }

    /// <summary>
    /// Update an address record
    /// </summary>
    /// <param name="addressId"></param>
    /// <param name="inputModel"></param>
    /// <returns></returns>
    [HttpPut("{addressId}")]
    [Produces(typeof(AddressModel))]
    public async Task<IActionResult> PutAsync(int addressId, [FromBody] AddressUpdateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The address input model cannot be null.");
        }

        if (addressId < 0)
        {
            return BadRequest("The address id must be a positive integer.");
        }

        if (addressId != inputModel.Id)
        {
            return BadRequest("The address id parameter must match the id of the address update request payload.");
        }

        var (addressModel, errors) = await _updateAddressService.UpdateAsync(inputModel).ConfigureAwait(false);

        if (errors.Any())
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return Ok(addressModel);

    }
}