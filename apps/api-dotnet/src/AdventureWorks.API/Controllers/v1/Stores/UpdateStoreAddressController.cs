using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for updating a store address's address type.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/addresses", Name = "UpdateStoreAddressControllerV1")]
public sealed class UpdateStoreAddressController : ControllerBase
{
    private readonly ILogger<UpdateStoreAddressController> _logger;
    private readonly IMediator _mediator;

    public UpdateStoreAddressController(
        ILogger<UpdateStoreAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Changes the address type of an existing store address.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="addressId">Address identifier.</param>
    /// <param name="addressTypeId">Existing address type identifier (the row to be replaced).</param>
    /// <param name="inputModel">The update payload (carries the new AddressTypeId).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated address.</returns>
    /// <response code="200">Address updated successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store address not found.</response>
    [HttpPatch("{addressId:int}/{addressTypeId:int}")]
    [ProducesResponseType(typeof(StoreAddressModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchAsync(
        int storeId,
        int addressId,
        int addressTypeId,
        [FromBody] StoreAddressUpdateModel? inputModel,
        CancellationToken cancellationToken = default)
    {
        if (inputModel == null)
        {
            return BadRequest("The store address input model cannot be null.");
        }

        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        if (addressId <= 0)
        {
            return BadRequest("A valid address id must be specified.");
        }

        if (addressTypeId <= 0)
        {
            return BadRequest("A valid address type id must be specified.");
        }

        _logger.LogInformation(
            "Updating address type for store {StoreId} (AddressId={AddressId}, AddressTypeId={AddressTypeId} -> {NewAddressTypeId})",
            storeId, addressId, addressTypeId, inputModel.AddressTypeId);

        var command = new UpdateStoreAddressTypeCommand
        {
            StoreId = storeId,
            AddressId = addressId,
            CurrentAddressTypeId = addressTypeId,
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(command, cancellationToken);

        var model = await _mediator.Send(new ReadStoreAddressQuery
        {
            StoreId = storeId,
            AddressId = addressId,
            AddressTypeId = inputModel.AddressTypeId
        }, cancellationToken);

        if (model is null)
        {
            return NotFound(
                $"Store address not found after update for StoreId={storeId}, AddressId={addressId}, AddressTypeId={inputModel.AddressTypeId}.");
        }

        return Ok(model);
    }
}
