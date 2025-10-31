using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadSalesDashboardQueryHandlerTests : UnitTestBase
{
    private readonly Mock<ISalesDashboardRepository> _mockDashboardRepository = new();
    private ReadSalesDashboardQueryHandler _sut;

    public ReadSalesDashboardQueryHandlerTests()
    {
        _sut = new ReadSalesDashboardQueryHandler(_mockDashboardRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesDashboardQueryHandler(null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("dashboardRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null_Async()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_dashboard_model_Async()
    {
        var testModel = new SalesDashboardModel
        {
            TotalRevenue = 123456789.99m,
            OrderCount = 31465,
            AverageOrderValue = 3922.05m,
            TopPerformers =
            [
                new DashboardTopPerformerModel { SalesPersonId = 275, Name = "Michael Blythe", Revenue = 9293903.00m, OrderCount = 450 },
                new DashboardTopPerformerModel { SalesPersonId = 276, Name = "Linda Mitchell", Revenue = 8845979.00m, OrderCount = 418 }
            ],
            TerritoryBreakdown =
            [
                new DashboardTerritoryModel { TerritoryId = 4, Name = "Southwest", CountryCode = "US", OrderCount = 3421, Revenue = 22000000m },
                new DashboardTerritoryModel { TerritoryId = 1, Name = "Northwest", CountryCode = "US", OrderCount = 3100, Revenue = 19000000m }
            ],
            MonthlySalesTrend =
            [
                new DashboardMonthlySalesTrendModel { Year = 2012, Month = 7, OrderCount = 210, Revenue = 800000m },
                new DashboardMonthlySalesTrendModel { Year = 2012, Month = 8, OrderCount = 230, Revenue = 850000m }
            ]
        };

        _mockDashboardRepository
            .Setup(x => x.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testModel);

        var result = await _sut.Handle(new ReadSalesDashboardQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the repository returned a model");
            result.TotalRevenue.Should().Be(123456789.99m);
            result.OrderCount.Should().Be(31465);
            result.TopPerformers.Should().HaveCount(2, "because two top performers were returned");
            result.TerritoryBreakdown.Should().HaveCount(2, "because two territories were returned");
            result.MonthlySalesTrend.Should().HaveCount(2, "because two monthly trend entries were returned");
        }
    }
}
