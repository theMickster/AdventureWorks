using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Application.Interfaces.Repositories;

namespace AdventureWorks.API.Controllers;

/// <summary>
/// Retrieve all products
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    /// <summary>
    /// Retrieve all products
    /// </summary>
    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieve all products
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<List<Product>>> GetAllProductAsync()
    {
        return await _productRepository.GetAllProductsAsync();
    }

}