using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates retrieving product price history.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ReadProductPriceHistoryControllerV1")]
public sealed class ReadProductPriceHistoryController : ControllerBase
{
    private readonly ILogger<ReadProductPriceHistoryController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product price history.
    /// </summary>
    public ReadProductPriceHistoryController(
        ILogger<ReadProductPriceHistoryController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves combined cost and list price history for a specific product
    /// </summary>
    /// <param name="id">the product id</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpGet("{id:int}/price-history")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductPriceHistoryModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPriceHistoryAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid product id must be specified.");
        }

        _logger.LogInformation("Reading price history for product {ProductId}", id);

        var result = await _mediator.Send(new ReadProductPriceHistoryQuery { ProductId = id }, cancellationToken);

        return Ok(result);
    }
}
