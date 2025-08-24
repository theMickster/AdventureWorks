using AdventureWorks.Application.Features.ProductReview.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductReviews;

/// <summary>
/// The controller that coordinates deleting a product review.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ProductReview")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/product-reviews/{reviewId:int}", Name = "DeleteSingleProductReviewControllerV1")]
public sealed class DeleteSingleProductReviewController : ControllerBase
{
    private readonly ILogger<DeleteSingleProductReviewController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates deleting a product review.
    /// </summary>
    public DeleteSingleProductReviewController(
        ILogger<DeleteSingleProductReviewController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Permanently deletes an existing product review.
    /// </summary>
    /// <param name="reviewId">The unique product review identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Product review deleted successfully</response>
    /// <response code="400">Invalid review ID</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Product review not found</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductReviewAsync(
        int reviewId,
        CancellationToken cancellationToken = default)
    {
        if (reviewId <= 0)
        {
            return BadRequest("The review ID must be a positive integer.");
        }

        await _mediator.Send(new DeleteProductReviewCommand { Id = reviewId }, cancellationToken);

        return NoContent();
    }
}
