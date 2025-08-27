using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for retrieving a store's year-to-date sales performance summary.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/performance", Name = "ReadStorePerformanceControllerV1")]
[Authorize]
public sealed class ReadStorePerformanceController : ControllerBase
{
    private readonly ILogger<ReadStorePerformanceController> _logger;
    private readonly IMediator _mediator;

    public ReadStorePerformanceController(
        ILogger<ReadStorePerformanceController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the year-to-date sales performance summary for the specified store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The store's YTD revenue, order count, average order value, and customer count.</returns>
    /// <response code="200">Performance summary retrieved successfully.</response>
    /// <response code="400">Invalid store ID supplied.</response>
    /// <response code="404">Store performance not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet(Name = "GetStorePerformance")]
    [ProducesResponseType(typeof(StorePerformanceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(int storeId, CancellationToken cancellationToken = default)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation("Retrieving performance summary for store {StoreId}", storeId);

        var model = await _mediator.Send(new ReadStorePerformanceQuery { StoreId = storeId }, cancellationToken);

        if (model is null)
        {
            return NotFound("Unable to locate store performance.");
        }

        return Ok(model);
    }
}
