using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for retrieving the paged customer list for a single store.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/customers", Name = "ReadStoreCustomersControllerV1")]
[Authorize]
public sealed class ReadStoreCustomersController : ControllerBase
{
    private readonly ILogger<ReadStoreCustomersController> _logger;
    private readonly IMediator _mediator;

    public ReadStoreCustomersController(
        ILogger<ReadStoreCustomersController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a paged list of customers for the specified store, sorted by lifetime spend
    /// (descending) by default. Supports sorting by lifetimeSpend, personName, orderCount, or
    /// lastOrderDate via the <c>orderBy</c> query parameter.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="parameters">Paging, sort column, and sort order query string parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The paged customer search result with per-customer order aggregates.</returns>
    /// <response code="200">Customer list retrieved successfully.</response>
    /// <response code="400">Invalid store ID supplied.</response>
    /// <response code="404">Store does not exist.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet(Name = "GetStoreCustomers")]
    [ProducesResponseType(typeof(StoreCustomerSearchResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(
        int storeId,
        [FromQuery] StoreCustomerParameter parameters,
        CancellationToken cancellationToken = default)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation("Retrieving customer list for store {StoreId}", storeId);

        var model = await _mediator.Send(
            new ReadStoreCustomerListQuery { StoreId = storeId, Parameters = parameters },
            cancellationToken);

        if (model is null)
        {
            return NotFound("Unable to locate store customers.");
        }

        return Ok(model);
    }
}
