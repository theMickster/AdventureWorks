using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates retrieving product categories.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ReadProductCategoriesControllerV1")]
public sealed class ReadProductCategoriesController : ControllerBase
{
    private readonly ILogger<ReadProductCategoriesController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product categories.
    /// </summary>
    public ReadProductCategoriesController(
        ILogger<ReadProductCategoriesController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all product categories
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductCategoryModel>))]
    public async Task<IActionResult> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reading product categories");

        var result = await _mediator.Send(new ReadProductCategoriesQuery(), cancellationToken);

        return Ok(result);
    }
}
