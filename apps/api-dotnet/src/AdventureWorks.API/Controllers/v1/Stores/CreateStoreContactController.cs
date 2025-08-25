using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for adding a contact to a store.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/contacts", Name = "CreateStoreContactControllerV1")]
public sealed class CreateStoreContactController : ControllerBase
{
    private readonly ILogger<CreateStoreContactController> _logger;
    private readonly IMediator _mediator;

    public CreateStoreContactController(
        ILogger<CreateStoreContactController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a new contact (person + contact type) to the specified store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="inputModel">The contact payload.</param>
    /// <returns>The newly created contact.</returns>
    /// <response code="201">Contact created successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(StoreContactModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostAsync(
        int storeId,
        [FromBody] StoreContactCreateModel? inputModel,
        CancellationToken cancellationToken = default)
    {
        if (inputModel == null)
        {
            return BadRequest("The store contact input model cannot be null.");
        }

        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation(
            "Creating contact for store {StoreId} (PersonId={PersonId}, ContactTypeId={ContactTypeId})",
            storeId, inputModel.PersonId, inputModel.ContactTypeId);

        var command = new AddStoreContactCommand
        {
            StoreId = storeId,
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow,
            RowGuid = Guid.NewGuid()
        };

        await _mediator.Send(command, cancellationToken);

        var model = await _mediator.Send(new ReadStoreContactQuery
        {
            StoreId = storeId,
            PersonId = inputModel.PersonId,
            ContactTypeId = inputModel.ContactTypeId
        }, cancellationToken);

        if (model is null)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve newly created store contact for StoreId={storeId}, PersonId={inputModel.PersonId}, ContactTypeId={inputModel.ContactTypeId}.");
        }

        return CreatedAtRoute(
            "GetStoreContactByCompositeKey",
            new { storeId, personId = inputModel.PersonId, contactTypeId = inputModel.ContactTypeId },
            model);
    }
}
