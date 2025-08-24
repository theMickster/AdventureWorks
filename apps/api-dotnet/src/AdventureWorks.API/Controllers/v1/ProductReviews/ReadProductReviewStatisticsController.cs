using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Models.Features.ProductReview;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductReviews;

/// <summary>
/// The controller that coordinates retrieving product review statistics.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ProductReview")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products/{productId:int}/reviews/statistics", Name = "ReadProductReviewStatisticsControllerV1")]
public sealed class ReadProductReviewStatisticsController : ControllerBase
{
    private readonly ILogger<ReadProductReviewStatisticsController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product review statistics.
    /// </summary>
    /// <remarks></remarks>
    public ReadProductReviewStatisticsController(
        ILogger<ReadProductReviewStatisticsController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves aggregate statistics for a given product's reviews.
    /// </summary>
    /// <param name="productId">the unique product identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductReviewStatisticsModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductReviewStatisticsAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return BadRequest("A valid product id must be specified.");
        }

        var result = await _mediator.Send(new ReadProductReviewStatisticsQuery
        {
            ProductId = productId
        }, cancellationToken);

        return Ok(result);
    }
}
