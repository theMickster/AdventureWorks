using AdventureWorks.Application.Features.Sales.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for deleting an address from a store.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/addresses", Name = "DeleteStoreAddressControllerV1")]
public sealed class DeleteStoreAddressController : ControllerBase
{
    private readonly ILogger<DeleteStoreAddressController> _logger;
    private readonly IMediator _mediator;

    public DeleteStoreAddressController(
        ILogger<DeleteStoreAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Deletes the address identified by the composite key.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="addressId">Address identifier.</param>
    /// <param name="addressTypeId">Address type identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Address deleted successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store address not found.</response>
    [HttpDelete("{addressId:int}/{addressTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        int storeId,
        int addressId,
        int addressTypeId,
        CancellationToken cancellationToken = default)
    {
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
            "Deleting address for store {StoreId} (AddressId={AddressId}, AddressTypeId={AddressTypeId})",
            storeId, addressId, addressTypeId);

        await _mediator.Send(new DeleteStoreAddressCommand
        {
            StoreId = storeId,
            AddressId = addressId,
            AddressTypeId = addressTypeId
        }, cancellationToken);

        return NoContent();
    }
}
