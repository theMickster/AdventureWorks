using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Models.Features.ProductReview;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductReviews;

/// <summary>
/// The controller that coordinates creating a product review.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ProductReview")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/product-reviews", Name = "CreateProductReviewControllerV1")]
public sealed class CreateProductReviewController : ControllerBase
{
    private readonly ILogger<CreateProductReviewController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates creating a product review.
    /// </summary>
    public CreateProductReviewController(
        ILogger<CreateProductReviewController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new product review.
    /// </summary>
    /// <param name="model">the product review to create</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProductReviewAsync(
        [FromBody] ProductReviewCreateModel? model,
        CancellationToken cancellationToken = default)
    {
        if (model == null)
        {
            return BadRequest("The product review input model cannot be null.");
        }

        var cmd = new CreateProductReviewCommand
        {
            Model = model,
            ModifiedDate = DateTime.UtcNow
        };

        var newId = await _mediator.Send(cmd, cancellationToken);

        return CreatedAtRoute("ReadSingleProductReviewControllerV1", new { version = "1", reviewId = newId }, null);
    }
}
