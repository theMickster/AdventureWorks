using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesPersonPerformanceQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private ReadSalesPersonPerformanceQueryHandler _sut;

    public ReadSalesPersonPerformanceQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesPersonEntityToPerformanceModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadSalesPersonPerformanceQueryHandler(_mapper, _mockSalesPersonRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesPersonPerformanceQueryHandler(null!, _mockSalesPersonRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesPersonPerformanceQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesPersonRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null_Async()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_null_when_entity_not_found_Async()
    {
        const int salesPersonId = 999999;

        _mockSalesPersonRepository
            .Setup(x => x.GetSalesPersonWithPerformanceDataAsync(salesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesPersonEntity)null!);

        var result = await _sut.Handle(new ReadSalesPersonPerformanceQuery { Id = salesPersonId }, CancellationToken.None);

        result.Should().BeNull("because the entity was not found in the repository");
    }

    [Fact]
    public async Task Handle_returns_valid_model_with_complete_data_Async()
    {
        const int salesPersonId = 275;
        const decimal salesYtd = 1750000m;
        const decimal salesLastYear = 1500000m;
        const decimal salesQuota = 300000m;
        const decimal bonus = 4000m;
        const decimal commissionPct = 0.012m;
        const int expectedOrderCount = 450;
        const decimal expectedTotalRevenue = 10475367.08m;

        var quotaHistory = new List<SalesPersonQuotaHistoryEntity>
        {
            new() { BusinessEntityId = salesPersonId, QuotaDate = new DateTime(2011, 5, 31), SalesQuota = 28000m },
            new() { BusinessEntityId = salesPersonId, QuotaDate = new DateTime(2011, 8, 31), SalesQuota = 7000m }
        };

        var territoryHistory = new List<SalesTerritoryHistory>
        {
            new()
            {
                BusinessEntityId = salesPersonId,
                TerritoryId = 2,
                StartDate = new DateTime(2005, 7, 1),
                EndDate = null,
                TerritoryEntity = new SalesTerritoryEntity { TerritoryId = 2, Name = "Northeast" }
            }
        };

        var entity = new SalesPersonEntity
        {
            BusinessEntityId = salesPersonId,
            SalesYtd = salesYtd,
            SalesLastYear = salesLastYear,
            SalesQuota = salesQuota,
            Bonus = bonus,
            CommissionPct = commissionPct,
            SalesPersonQuotaHistory = quotaHistory,
            SalesTerritoryHistory = territoryHistory
        };

        _mockSalesPersonRepository
            .Setup(x => x.GetSalesPersonWithPerformanceDataAsync(salesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockSalesPersonRepository
            .Setup(x => x.GetSalesPersonOrderAggregatesAsync(salesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedOrderCount, expectedTotalRevenue));

        var result = await _sut.Handle(new ReadSalesPersonPerformanceQuery { Id = salesPersonId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the entity was found");
            result!.SalesYtd.Should().Be(salesYtd);
            result.SalesLastYear.Should().Be(salesLastYear);
            result.SalesQuota.Should().Be(salesQuota);
            result.Bonus.Should().Be(bonus);
            result.CommissionPct.Should().Be(commissionPct);
            result.OrderCount.Should().Be(expectedOrderCount, "because OrderCount is set from aggregate query");
            result.TotalRevenue.Should().Be(expectedTotalRevenue, "because TotalRevenue is set from aggregate query");
            result.QuotaHistory.Should().HaveCount(2, "because two quota history entries were loaded");
            result.TerritoryHistory.Should().HaveCount(1, "because one territory history entry was loaded");
            result.TerritoryHistory[0].TerritoryName.Should().Be("Northeast", "because TerritoryName maps from TerritoryEntity.Name");
        }
    }

    [Fact]
    public async Task Handle_returns_null_sales_quota_when_not_set_Async()
    {
        const int salesPersonId = 288;

        var entity = new SalesPersonEntity
        {
            BusinessEntityId = salesPersonId,
            SalesQuota = null,
            SalesPersonQuotaHistory = new List<SalesPersonQuotaHistoryEntity>(),
            SalesTerritoryHistory = new List<SalesTerritoryHistory>()
        };

        _mockSalesPersonRepository
            .Setup(x => x.GetSalesPersonWithPerformanceDataAsync(salesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockSalesPersonRepository
            .Setup(x => x.GetSalesPersonOrderAggregatesAsync(salesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0m));

        var result = await _sut.Handle(new ReadSalesPersonPerformanceQuery { Id = salesPersonId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.SalesQuota.Should().BeNull("because SalesQuota is nullable and was not set");
            result.OrderCount.Should().Be(0);
            result.TotalRevenue.Should().Be(0m);
        }
    }

    [Fact]
    public async Task Handle_does_not_call_aggregate_when_entity_not_found_Async()
    {
        const int salesPersonId = 999999;

        _mockSalesPersonRepository
            .Setup(x => x.GetSalesPersonWithPerformanceDataAsync(salesPersonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesPersonEntity)null!);

        var result = await _sut.Handle(new ReadSalesPersonPerformanceQuery { Id = salesPersonId }, CancellationToken.None);

        result.Should().BeNull("because the entity was not found in the repository");
    }
}
