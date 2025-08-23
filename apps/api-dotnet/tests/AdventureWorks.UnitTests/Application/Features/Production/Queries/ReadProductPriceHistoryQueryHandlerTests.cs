using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductPriceHistoryQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly ReadProductPriceHistoryQueryHandler _sut;

    public ReadProductPriceHistoryQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductListPriceHistoryToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadProductPriceHistoryQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_combined_price_history_sorted_by_start_dateAsync()
    {
        var listPriceHistory = ProductionDomainFixtures.GetMockListPriceHistory(1);
        var costHistory = ProductionDomainFixtures.GetMockCostHistory(1);

        _mockProductRepository.Setup(x => x.GetListPriceHistoryByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listPriceHistory.AsReadOnly());

        _mockProductRepository.Setup(x => x.GetCostHistoryByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(costHistory.AsReadOnly());

        var result = await _sut.Handle(new ReadProductPriceHistoryQuery { ProductId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(4);
            result.Should().BeInAscendingOrder(h => h.StartDate);
            result.Count(h => h.Type == "list").Should().Be(2);
            result.Count(h => h.Type == "cost").Should().Be(2);
        }
    }

    [Fact]
    public async Task Handle_returns_empty_when_no_historyAsync()
    {
        _mockProductRepository.Setup(x => x.GetListPriceHistoryByProductIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductListPriceHistory>().AsReadOnly());

        _mockProductRepository.Setup(x => x.GetCostHistoryByProductIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCostHistory>().AsReadOnly());

        var result = await _sut.Handle(new ReadProductPriceHistoryQuery { ProductId = 999 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Count.Should().Be(0);
    }
}
