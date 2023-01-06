using AdventureWorks.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AdventureWorks.Domain.Entities;

namespace AdventureWorks.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        //GET api/v1/[controller]/
        [Route("")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Product>), (int) HttpStatusCode.OK)]
        public async Task<ActionResult<List<Product>>> GetAllProductAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

    }
}