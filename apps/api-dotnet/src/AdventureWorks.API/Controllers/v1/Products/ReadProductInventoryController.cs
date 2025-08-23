using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates retrieving product inventory by location.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ReadProductInventoryControllerV1")]
public sealed class ReadProductInventoryController : ControllerBase
{
    private readonly ILogger<ReadProductInventoryController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product inventory by location.
    /// </summary>
    public ReadProductInventoryController(
        ILogger<ReadProductInventoryController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves inventory for a specific product across all locations
    /// </summary>
    /// <param name="id">the product id</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpGet("{id:int}/inventory")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductInventoryModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInventoryAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid product id must be specified.");
        }

        _logger.LogInformation("Reading inventory for product {ProductId}", id);

        var result = await _mediator.Send(new ReadProductInventoryQuery { ProductId = id }, cancellationToken);

        return Ok(result);
    }
}
