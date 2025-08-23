using AdventureWorks.Application.Features.Production.Commands;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Models.Features.Production;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// The controller that coordinates creating product information.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "CreateProductControllerV1")]
public sealed class CreateProductController : ControllerBase
{
    private readonly ILogger<CreateProductController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// The controller that coordinates creating product information.
    /// </summary>
    public CreateProductController(
        ILogger<CreateProductController> logger,
        IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="inputModel">the product to create</param>
    /// <param name="cancellationToken">cancellation token</param>
    [HttpPost]
    [Produces(typeof(ProductDetailModel))]
    public async Task<IActionResult> PostAsync([FromBody] ProductCreateModel? inputModel, CancellationToken cancellationToken = default)
    {
        if (inputModel == null)
        {
            return BadRequest("The product input model cannot be null.");
        }

        _logger.LogInformation("Creating product {ProductName}", inputModel.Name);

        var cmd = new CreateProductCommand { Model = inputModel, ModifiedDate = DateTime.UtcNow, RowGuid = Guid.NewGuid() };

        var productId = await _mediator.Send(cmd, cancellationToken);
        var model = await _mediator.Send(new ReadProductQuery { Id = productId }, cancellationToken);

        return CreatedAtRoute("GetProductById", new { id = model!.Id }, model);
    }
}
