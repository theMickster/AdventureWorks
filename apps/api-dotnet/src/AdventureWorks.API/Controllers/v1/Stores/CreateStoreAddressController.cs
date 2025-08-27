using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for adding an address to a store.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/addresses", Name = "CreateStoreAddressControllerV1")]
public sealed class CreateStoreAddressController : ControllerBase
{
    private readonly ILogger<CreateStoreAddressController> _logger;
    private readonly IMediator _mediator;

    public CreateStoreAddressController(
        ILogger<CreateStoreAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a new address (address + address type) to the specified store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="inputModel">The address payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created address.</returns>
    /// <response code="201">Address created successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(StoreAddressModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostAsync(
        int storeId,
        [FromBody] StoreAddressCreateModel? inputModel,
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

        _logger.LogInformation(
            "Creating address for store {StoreId} (AddressId={AddressId}, AddressTypeId={AddressTypeId})",
            storeId, inputModel.AddressId, inputModel.AddressTypeId);

        var command = new AddStoreAddressCommand
        {
            StoreId = storeId,
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow,
            RowGuid = Guid.NewGuid()
        };

        await _mediator.Send(command, cancellationToken);

        var model = await _mediator.Send(new ReadStoreAddressQuery
        {
            StoreId = storeId,
            AddressId = inputModel.AddressId,
            AddressTypeId = inputModel.AddressTypeId
        }, cancellationToken);

        if (model is null)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve newly created store address for StoreId={storeId}, AddressId={inputModel.AddressId}, AddressTypeId={inputModel.AddressTypeId}.");
        }

        return CreatedAtRoute(
            "GetStoreAddressByCompositeKey",
            new { storeId, addressId = inputModel.AddressId, addressTypeId = inputModel.AddressTypeId },
            model);
    }
}
