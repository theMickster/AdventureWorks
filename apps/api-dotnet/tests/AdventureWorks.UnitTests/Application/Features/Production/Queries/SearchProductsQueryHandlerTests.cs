using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class SearchProductsQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly SearchProductsQueryHandler _sut;

    public SearchProductsQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductToListModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new SearchProductsQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_empty_result_when_no_matchesAsync()
    {
        _mockProductRepository.Setup(x => x.SearchProductsAsync(It.IsAny<ProductParameter>(), It.IsAny<ProductSearchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>().AsReadOnly(), 0));

        var query = new SearchProductsQuery
        {
            Parameters = new ProductParameter(),
            SearchModel = new ProductSearchModel { Color = "Purple" }
        };
        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_returns_filtered_resultsAsync()
    {
        var products = ProductionDomainFixtures.GetMockProducts().Take(1).ToList();

        _mockProductRepository.Setup(x => x.SearchProductsAsync(It.IsAny<ProductParameter>(), It.IsAny<ProductSearchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((products.AsReadOnly(), 1));

        var query = new SearchProductsQuery
        {
            Parameters = new ProductParameter(),
            SearchModel = new ProductSearchModel { CategoryId = 1 }
        };
        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(1);
            result.Results.Should().NotBeNull();
            result.Results!.Count.Should().Be(1);
        }
    }
}
