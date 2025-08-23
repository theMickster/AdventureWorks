using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for retrieving store address information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/addresses", Name = "ReadStoreAddressControllerV1")]
[Authorize]
public sealed class ReadStoreAddressController : ControllerBase
{
    private readonly ILogger<ReadStoreAddressController> _logger;
    private readonly IMediator _mediator;

    public ReadStoreAddressController(
        ILogger<ReadStoreAddressController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all addresses for a specific store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId</param>
    /// <returns>List of addresses with address type information</returns>
    /// <response code="200">Addresses retrieved successfully</response>
    /// <response code="400">Invalid store ID supplied</response>
    /// <response code="500">Internal server error</response>
    [HttpGet(Name = "GetStoreAddresses")]
    [ProducesResponseType(typeof(List<BusinessEntityAddressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync(int storeId, CancellationToken cancellationToken = default)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation("Retrieving all addresses for store {StoreId}", storeId);

        var query = new ReadStoreAddressListQuery { StoreId = storeId };

        var addresses = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation("Retrieved {Count} addresses for store {StoreId}", addresses.Count, storeId);

        return Ok(addresses);
    }
}
