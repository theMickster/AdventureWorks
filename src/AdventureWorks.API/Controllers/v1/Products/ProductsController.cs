using AdventureWorks.Application.Interfaces.Repositories;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.API.Controllers.v1.Products;

/// <summary>
/// Retrieve all products
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Products")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/products", Name = "ProductsControllerV1")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;

    /// <summary>
    /// Retrieve all products
    /// </summary>
    public ProductsController(ILogger<ProductsController> logger, IProductRepository productRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository)) ;
    }

    ///// <summary>
    ///// Retrieve all products
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet]
    //public Task<ActionResult<List<Product>>> GetAllProductAsync()
    //{
    //    throw new NotImplementedException("Endpoint is not implemented.");
    //}
}