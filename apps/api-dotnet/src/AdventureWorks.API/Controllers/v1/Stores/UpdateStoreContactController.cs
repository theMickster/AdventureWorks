using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for updating a store contact's contact type.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/contacts", Name = "UpdateStoreContactControllerV1")]
public sealed class UpdateStoreContactController : ControllerBase
{
    private readonly ILogger<UpdateStoreContactController> _logger;
    private readonly IMediator _mediator;

    public UpdateStoreContactController(
        ILogger<UpdateStoreContactController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Changes the contact type of an existing store contact.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="personId">Person identifier.</param>
    /// <param name="contactTypeId">Existing contact type identifier (the row to be replaced).</param>
    /// <param name="inputModel">The update payload (carries the new ContactTypeId).</param>
    /// <returns>The updated contact.</returns>
    /// <response code="200">Contact updated successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store contact not found.</response>
    [HttpPatch("{personId:int}/{contactTypeId:int}")]
    [ProducesResponseType(typeof(StoreContactModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchAsync(
        int storeId,
        int personId,
        int contactTypeId,
        [FromBody] StoreContactUpdateModel? inputModel,
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

        if (personId <= 0)
        {
            return BadRequest("A valid person id must be specified.");
        }

        if (contactTypeId <= 0)
        {
            return BadRequest("A valid contact type id must be specified.");
        }

        _logger.LogInformation(
            "Updating contact type for store {StoreId} (PersonId={PersonId}, ContactTypeId={ContactTypeId} -> {NewContactTypeId})",
            storeId, personId, contactTypeId, inputModel.ContactTypeId);

        var command = new UpdateStoreContactTypeCommand
        {
            StoreId = storeId,
            PersonId = personId,
            CurrentContactTypeId = contactTypeId,
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow
        };

        await _mediator.Send(command, cancellationToken);

        var model = await _mediator.Send(new ReadStoreContactQuery
        {
            StoreId = storeId,
            PersonId = personId,
            ContactTypeId = inputModel.ContactTypeId
        }, cancellationToken);

        if (model is null)
        {
            return NotFound(
                $"Store contact not found after update for StoreId={storeId}, PersonId={personId}, ContactTypeId={inputModel.ContactTypeId}.");
        }

        return Ok(model);
    }
}
