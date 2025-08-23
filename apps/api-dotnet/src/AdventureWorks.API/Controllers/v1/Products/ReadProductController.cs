using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates retrieving a single product by its identifier.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ReadProductControllerV1")]
public sealed class ReadProductController : ControllerBase
{
    private readonly ILogger<ReadProductController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving a single product by its identifier.
    /// </summary>
    public ReadProductController(
        ILogger<ReadProductController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a product using its unique identifier
    /// </summary>
    /// <param name="id">the unique product identifier</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpGet("{id:int}", Name = "GetProductById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDetailModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid product id must be specified.");
        }

        _logger.LogInformation("Reading product {ProductId}", id);

        var model = await _mediator.Send(new ReadProductQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the product.") : Ok(model);
    }
}
