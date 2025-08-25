using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for retrieving store contact information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/contacts", Name = "ReadStoreContactControllerV1")]
[Authorize]
public sealed class ReadStoreContactController : ControllerBase
{
    private readonly ILogger<ReadStoreContactController> _logger;
    private readonly IMediator _mediator;

    public ReadStoreContactController(
        ILogger<ReadStoreContactController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all contacts for a specific store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId</param>
    /// <returns>List of contacts with contact type information</returns>
    /// <response code="200">Contacts retrieved successfully</response>
    /// <response code="400">Invalid store ID supplied</response>
    /// <response code="500">Internal server error</response>
    [HttpGet(Name = "GetStoreContacts")]
    [ProducesResponseType(typeof(List<StoreContactModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync(int storeId)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation("Retrieving all contacts for store {StoreId}", storeId);

        var query = new ReadStoreContactListQuery { StoreId = storeId };

        var contacts = await _mediator.Send(query);

        _logger.LogInformation("Retrieved {Count} contacts for store {StoreId}", contacts.Count, storeId);

        return Ok(contacts);
    }

    /// <summary>
    /// Retrieves a single store contact by its composite key.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="personId">Person identifier.</param>
    /// <param name="contactTypeId">Contact type identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The store contact matching the composite key.</returns>
    /// <response code="200">Contact retrieved successfully.</response>
    /// <response code="400">Invalid input.</response>
    /// <response code="404">Store contact not found.</response>
    [HttpGet("{personId:int}/{contactTypeId:int}", Name = "GetStoreContactByCompositeKey")]
    [ProducesResponseType(typeof(StoreContactModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCompositeKeyAsync(
        int storeId,
        int personId,
        int contactTypeId,
        CancellationToken cancellationToken = default)
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

        var model = await _mediator.Send(new ReadStoreContactQuery
        {
            StoreId = storeId,
            PersonId = personId,
            ContactTypeId = contactTypeId
        }, cancellationToken);

        return model is null ? NotFound("Unable to locate the store contact.") : Ok(model);
    }
}
