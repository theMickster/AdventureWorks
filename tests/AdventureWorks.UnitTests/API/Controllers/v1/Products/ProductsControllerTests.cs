using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ProductsControllerTests: UnitTestBase
{
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly Mock<ILogger<ProductsController>> _mockLogger = new();
    private readonly ProductsController _sut;

    public ProductsControllerTests()
    {
        _sut = new ProductsController(_mockLogger.Object, _mockProductRepository.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ProductsController(null!, _mockProductRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ProductsController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("productRepository");
        }
    }

    [Fact]
    public void GetAllProductAsync_throws_not_implemented_exception()
    {
        _ = ((Action)(() => _ = _sut.GetAllProductAsync()))
            .Should().Throw<NotImplementedException>("because we expect a not implemented exception.");
    }
}