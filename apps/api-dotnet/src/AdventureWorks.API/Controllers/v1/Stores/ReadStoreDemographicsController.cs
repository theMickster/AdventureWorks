using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for retrieving the parsed store demographics survey.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/demographics", Name = "ReadStoreDemographicsControllerV1")]
[Authorize]
public sealed class ReadStoreDemographicsController : ControllerBase
{
    private readonly ILogger<ReadStoreDemographicsController> _logger;
    private readonly IMediator _mediator;

    public ReadStoreDemographicsController(
        ILogger<ReadStoreDemographicsController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the demographics survey for the specified store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The store demographics, with survey fields null when unavailable.</returns>
    /// <response code="200">Demographics retrieved successfully.</response>
    /// <response code="400">Invalid store ID supplied.</response>
    /// <response code="404">Store demographics not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet(Name = "GetStoreDemographics")]
    [ProducesResponseType(typeof(StoreDemographicsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(int storeId, CancellationToken cancellationToken = default)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation("Retrieving demographics for store {StoreId}", storeId);

        var model = await _mediator.Send(new ReadStoreDemographicsQuery { StoreId = storeId }, cancellationToken);

        if (model is null)
        {
            return NotFound("Unable to locate store demographics.");
        }

        return Ok(model);
    }
}
