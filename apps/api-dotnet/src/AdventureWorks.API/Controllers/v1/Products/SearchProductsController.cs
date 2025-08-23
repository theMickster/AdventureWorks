using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates searching for products with filter criteria.
/// </summary>
/// <remarks>
/// This controller uses POST to accept a search body but performs a read-only query.
/// It is intentionally unauthenticated as product catalog data is public.
/// Per project security policy exception: POST verbs used purely for query filtering
/// (no state mutation) do not require authorization.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "SearchProductsControllerV1")]
public sealed class SearchProductsController : ControllerBase
{
    private readonly ILogger<SearchProductsController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates searching for products with filter criteria.
    /// </summary>
    public SearchProductsController(
        ILogger<SearchProductsController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>Searches products using the provided filter criteria. Read-only; no authentication required.</summary>
    /// <param name="parameters">product pagination query string</param>
    /// <param name="searchModel">the product search input model</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpPost]
    [Route("search", Name = "SearchProductsAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchProductsAsync([FromQuery] ProductParameter parameters, [FromBody] ProductSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        var searchResult = await _mediator.Send(new SearchProductsQuery { Parameters = parameters, SearchModel = searchModel }, cancellationToken);

        if (searchResult.Results is null or { Count: 0 })
        {
            _logger.LogInformation(
                "No results found for product search. Status={Status} Operation={Operation} ErrCode={ErrCode} ServiceId={ServiceId} {@Parameters}",
                AppLoggingConstants.StatusNotFound,
                "ProductSearchAsync",
                AppLoggingConstants.HttpGetRequestErrorCode,
                "AdventureWorksApi",
                parameters);
        }

        return Ok(searchResult);
    }
}
