using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Models.Features.Purchasing;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.PurchaseOrders;

/// <summary>
/// The controller that coordinates retrieving purchase order information.
/// Authenticated users may read purchase order detail — no additional role restriction per project auth policy.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "PurchaseOrder")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/purchase-orders", Name = "ReadPurchaseOrderControllerV1")]
public sealed class ReadPurchaseOrderController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving purchase order information.
    /// </summary>
    public ReadPurchaseOrderController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a single purchase order's full detail, including line items.
    /// </summary>
    /// <param name="purchaseOrderId">the purchase order primary key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The purchase order detail</returns>
    [HttpGet("{purchaseOrderId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PurchaseOrderDetailModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseOrderDetailAsync(
        [FromRoute] int purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        if (purchaseOrderId <= 0)
        {
            return BadRequest();
        }

        // A missing purchase order throws KeyNotFoundException, which ExceptionHandlerMiddleware
        // translates to a structured 404 body — deliberately left to bubble up rather than checked here.
        var result = await _mediator.Send(new ReadPurchaseOrderDetailQuery { PurchaseOrderId = purchaseOrderId }, cancellationToken);

        return Ok(result);
    }
}
