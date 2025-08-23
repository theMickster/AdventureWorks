using AdventureWorks.Application.Features.Production.Commands;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates updating product information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "UpdateProductControllerV1")]
public sealed class UpdateProductController : ControllerBase
{
    private readonly ILogger<UpdateProductController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates updating product information.
    /// </summary>
    public UpdateProductController(
        ILogger<UpdateProductController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Update a product record
    /// </summary>
    /// <param name="id">the product id</param>
    /// <param name="inputModel">the product update model</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpPut("{id:int}")]
    [Produces(typeof(ProductDetailModel))]
    public async Task<IActionResult> PutAsync(int id, [FromBody] ProductUpdateModel? inputModel, CancellationToken cancellationToken = default)
    {
        if (inputModel == null)
        {
            return BadRequest("The product input model cannot be null.");
        }

        if (id <= 0)
        {
            return BadRequest("The product id must be a positive integer.");
        }

        if (id != inputModel.Id)
        {
            return BadRequest("The product id parameter must match the id of the product update request payload.");
        }

        _logger.LogInformation("Updating product {ProductId}", id);

        var cmd = new UpdateProductCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow };
        await _mediator.Send(cmd, cancellationToken);
        var model = await _mediator.Send(new ReadProductQuery { Id = id }, cancellationToken);

        return Ok(model);
    }
}
