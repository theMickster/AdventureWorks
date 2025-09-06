using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.ProductModels;

/// <summary>
/// The controller that coordinates retrieving product model information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Production")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/product-models", Name = "ReadProductModelControllerV1")]
public sealed class ReadProductModelController : ControllerBase
{
    private readonly ILogger<ReadProductModelController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates retrieving product model information.
    /// </summary>
    /// <remarks></remarks>
    public ReadProductModelController(
        ILogger<ReadProductModelController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve a product model using its unique identifier
    /// </summary>
    /// <param name="id">the unique product model identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = "GetProductModelById")]
    [Produces(typeof(ProductModelDetailModel))]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest("A valid product model id must be specified.");
        }

        var model = await _mediator.Send(new ReadProductModelQuery { Id = id }, cancellationToken);

        return model is null ? NotFound("Unable to locate the product model.") : Ok(model);
    }

    /// <summary>
    /// Retrieve the complete product model list
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    [HttpGet(Name = "GetProductModels")]
    [Produces(typeof(List<ProductModelListModel>))]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken = default)
    {
        var model = await _mediator.Send(new ReadProductModelListQuery(), cancellationToken);

        if (model is not { Count: > 0 })
        {
            return NotFound("Unable to locate records in the product model list.");
        }

        return Ok(model);
    }
}
