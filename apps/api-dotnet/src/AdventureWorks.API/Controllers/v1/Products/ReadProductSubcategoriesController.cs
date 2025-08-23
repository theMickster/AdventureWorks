using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates retrieving product subcategories.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ReadProductSubcategoriesControllerV1")]
public sealed class ReadProductSubcategoriesController : ControllerBase
{
    private readonly ILogger<ReadProductSubcategoriesController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product subcategories.
    /// </summary>
    public ReadProductSubcategoriesController(
        ILogger<ReadProductSubcategoriesController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves product subcategories, optionally filtered by category
    /// </summary>
    /// <param name="categoryId">optional category id to filter subcategories</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpGet("subcategories")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductSubcategoryModel>))]
    public async Task<IActionResult> GetSubcategoriesAsync([FromQuery] int? categoryId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reading product subcategories for category {CategoryId}", categoryId);

        var result = await _mediator.Send(new ReadProductSubcategoriesQuery { CategoryId = categoryId }, cancellationToken);

        return Ok(result);
    }
}
