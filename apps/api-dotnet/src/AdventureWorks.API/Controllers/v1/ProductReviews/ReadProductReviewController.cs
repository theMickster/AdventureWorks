using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.ProductReview;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductReviews;

/// <summary>
/// The controller that coordinates retrieving product review information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "ProductReview")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products/{productId:int}/reviews", Name = "ReadProductReviewControllerV1")]
public sealed class ReadProductReviewController : ControllerBase
{
    private readonly ILogger<ReadProductReviewController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product review information.
    /// </summary>
    /// <remarks></remarks>
    public ReadProductReviewController(
        ILogger<ReadProductReviewController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a paged list of product reviews for a given product.
    /// </summary>
    /// <param name="productId">the unique product identifier</param>
    /// <param name="parameters">product review pagination query string</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductReviewSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductReviewListAsync(
        int productId,
        [FromQuery] ProductReviewParameter parameters,
        CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return BadRequest("A valid product id must be specified.");
        }

        var searchResult = await _mediator.Send(new ReadProductReviewListQuery
        {
            ProductId = productId,
            Parameters = parameters
        }, cancellationToken);

        return Ok(searchResult);
    }
}
