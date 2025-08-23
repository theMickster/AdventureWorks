using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly ReadProductListQueryHandler _sut;

    public ReadProductListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductToListModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadProductListQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_empty_result_when_no_productsAsync()
    {
        _mockProductRepository.Setup(x => x.GetProductsAsync(It.IsAny<ProductParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>().AsReadOnly(), 0));

        var query = new ReadProductListQuery { Parameters = new ProductParameter() };
        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(0);
            result.Results.Should().BeNull();
        }
    }

    [Fact]
    public async Task Handle_returns_paged_resultsAsync()
    {
        var products = ProductionDomainFixtures.GetMockProducts();

        _mockProductRepository.Setup(x => x.GetProductsAsync(It.IsAny<ProductParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((products.AsReadOnly(), 3));

        var query = new ReadProductListQuery { Parameters = new ProductParameter() };
        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(3);
            result.Results.Should().NotBeNull();
            result.Results!.Count.Should().Be(3);
            result.Results[0].Name.Should().Be("Mountain Bike");
        }
    }
}
