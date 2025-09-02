using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for retrieving the sales person assignment history for a store.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/sales-person-assignments", Name = "ReadStoreSalesPersonAssignmentControllerV1")]
public sealed class ReadStoreSalesPersonAssignmentController : ControllerBase
{
    private readonly ILogger<ReadStoreSalesPersonAssignmentController> _logger;
    private readonly IMediator _mediator;

    public ReadStoreSalesPersonAssignmentController(
        ILogger<ReadStoreSalesPersonAssignmentController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the full sales person assignment history for the specified store.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of sales person assignment history records, ordered by StartDate descending.</returns>
    /// <response code="200">Assignments retrieved successfully.</response>
    /// <response code="400">Invalid store ID supplied.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Store not found.</response>
    [HttpGet(Name = "GetStoreSalesPersonAssignments")]
    [ProducesResponseType(typeof(List<StoreSalesPersonAssignmentModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(
        int storeId,
        CancellationToken cancellationToken = default)
    {
        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation("Retrieving sales person assignment history for store {StoreId}", storeId);

        var assignments = await _mediator.Send(
            new ReadStoreSalesPersonAssignmentListQuery { StoreId = storeId },
            cancellationToken);

        return Ok(assignments);
    }
}
