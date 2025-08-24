using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Models.Features.ProductReview;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductReviews;

/// <summary>
/// The controller that coordinates updating a product review.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ProductReview")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/product-reviews/{reviewId:int}", Name = "UpdateProductReviewControllerV1")]
public sealed class UpdateProductReviewController : ControllerBase
{
    private readonly ILogger<UpdateProductReviewController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates updating a product review.
    /// </summary>
    public UpdateProductReviewController(
        ILogger<UpdateProductReviewController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Fully replaces the mutable fields of an existing product review.
    /// </summary>
    /// <param name="reviewId">The unique product review identifier.</param>
    /// <param name="model">The product review update payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated product review.</returns>
    /// <response code="200">Product review updated successfully</response>
    /// <response code="400">Invalid review ID or payload</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductReviewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProductReviewAsync(
        int reviewId,
        [FromBody] ProductReviewUpdateModel? model,
        CancellationToken cancellationToken = default)
    {
        if (reviewId <= 0)
        {
            return BadRequest("The review ID must be a positive integer.");
        }

        if (model == null)
        {
            return BadRequest("The product review input model cannot be null.");
        }

        model.Id = reviewId;

        var cmd = new UpdateProductReviewCommand { Model = model, ModifiedDate = DateTime.UtcNow };
        await _mediator.Send(cmd, cancellationToken);

        var result = await _mediator.Send(new ReadProductReviewQuery { Id = reviewId }, cancellationToken);

        return Ok(result);
    }
}
