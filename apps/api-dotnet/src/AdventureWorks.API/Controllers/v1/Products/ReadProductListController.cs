using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates retrieving a paginated list of products.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ReadProductListControllerV1")]
public sealed class ReadProductListController : ControllerBase
{
    private readonly ILogger<ReadProductListController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving a paginated list of products.
    /// </summary>
    public ReadProductListController(
        ILogger<ReadProductListController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a paged list of products
    /// </summary>
    /// <param name="parameters">product pagination query string</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductSearchResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductListAsync([FromQuery] ProductParameter parameters, CancellationToken cancellationToken = default)
    {
        var searchResult = await _mediator.Send(new ReadProductListQuery { Parameters = parameters }, cancellationToken);

        if (searchResult.Results is null or { Count: 0 })
        {
            _logger.LogInformation(
                "No results found for product list. Status={Status} Operation={Operation} ErrCode={ErrCode} ServiceId={ServiceId} {@Parameters}",
                AppLoggingConstants.StatusNotFound,
                "ProductListAsync",
                AppLoggingConstants.HttpGetRequestErrorCode,
                "AdventureWorksApi",
                parameters);
        }

        return Ok(searchResult);
    }
}
