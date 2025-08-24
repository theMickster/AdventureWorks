using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Models.Features.ProductReview;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductReviews;

/// <summary>
/// The controller that coordinates retrieving a single product review by its unique identifier.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ProductReview")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/product-reviews/{reviewId:int}", Name = "ReadSingleProductReviewControllerV1")]
public sealed class ReadSingleProductReviewController : ControllerBase
{
    private readonly ILogger<ReadSingleProductReviewController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving a single product review by its unique identifier.
    /// </summary>
    public ReadSingleProductReviewController(
        ILogger<ReadSingleProductReviewController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a single product review by its unique identifier.
    /// </summary>
    /// <param name="reviewId">the unique product review identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductReviewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductReviewAsync(
        int reviewId,
        CancellationToken cancellationToken = default)
    {
        if (reviewId <= 0)
        {
            return BadRequest("A valid product review id must be specified.");
        }

        var model = await _mediator.Send(new ReadProductReviewQuery { Id = reviewId }, cancellationToken);

        return model is null ? NotFound("Unable to locate the product review.") : Ok(model);
    }
}
