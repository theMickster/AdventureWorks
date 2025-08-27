using AdventureWorks.Application.Features.Sales.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for deleting a contact from a store.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/contacts", Name = "DeleteStoreContactControllerV1")]
public sealed class DeleteStoreContactController : ControllerBase
{
    private readonly ILogger<DeleteStoreContactController> _logger;
    private readonly IMediator _mediator;

    public DeleteStoreContactController(
        ILogger<DeleteStoreContactController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Deletes the contact identified by the composite key.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="personId">Person identifier.</param>
    /// <param name="contactTypeId">Contact type identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Contact deleted successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store contact not found.</response>
    [HttpDelete("{personId:int}/{contactTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default)
    {
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
            "Deleting contact for store {StoreId} (PersonId={PersonId}, ContactTypeId={ContactTypeId})",
            storeId, personId, contactTypeId);

        await _mediator.Send(new DeleteStoreContactCommand
        {
            StoreId = storeId,
            PersonId = personId,
            ContactTypeId = contactTypeId
        }, cancellationToken);

        return NoContent();
    }
}
