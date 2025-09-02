using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Stores;

/// <summary>
/// Controller for reassigning a sales person to a store.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Store")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/stores/{storeId:int}/sales-person-assignments", Name = "ReassignStoreSalesPersonControllerV1")]
public sealed class ReassignStoreSalesPersonController : ControllerBase
{
    private readonly ILogger<ReassignStoreSalesPersonController> _logger;
    private readonly IMediator _mediator;

    public ReassignStoreSalesPersonController(
        ILogger<ReassignStoreSalesPersonController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Reassigns a sales person to the specified store, closing the current open history row
    /// and inserting a new one.
    /// </summary>
    /// <param name="storeId">Store's BusinessEntityId.</param>
    /// <param name="inputModel">The assignment payload containing the new SalesPersonId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The full sales person assignment history for the store.</returns>
    /// <response code="201">Sales person reassigned successfully.</response>
    /// <response code="400">Invalid input or business rule violation.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Store not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(List<StoreSalesPersonAssignmentModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostAsync(
        int storeId,
        [FromBody] StoreSalesPersonAssignmentCreateModel? inputModel,
        CancellationToken cancellationToken = default)
    {
        if (inputModel == null)
        {
            return BadRequest("The sales person assignment input model cannot be null.");
        }

        if (storeId <= 0)
        {
            return BadRequest("A valid store id must be specified.");
        }

        _logger.LogInformation(
            "Reassigning sales person {SalesPersonId} to store {StoreId}",
            inputModel.SalesPersonId, storeId);

        var command = new ReassignStoreSalesPersonCommand
        {
            StoreId = storeId,
            Model = inputModel,
            AssignedDate = DateTime.UtcNow
        };

        await _mediator.Send(command, cancellationToken);

        var assignments = await _mediator.Send(
            new ReadStoreSalesPersonAssignmentListQuery { StoreId = storeId },
            cancellationToken);

        return CreatedAtRoute("GetStoreSalesPersonAssignments", new { storeId }, assignments);
    }
}
