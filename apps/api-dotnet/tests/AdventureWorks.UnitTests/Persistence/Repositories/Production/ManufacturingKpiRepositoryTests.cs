using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories.Production;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Production;

[ExcludeFromCodeCoverage]
public sealed class ManufacturingKpiRepositoryTests : PersistenceUnitTestBase
{
    private readonly ManufacturingKpiRepository _sut;

    public ManufacturingKpiRepositoryTests()
    {
        _sut = new ManufacturingKpiRepository(DbContext);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        typeof(ManufacturingKpiRepository)
            .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetManufacturingKpisAsync_with_no_work_orders_returns_zero_metrics()
    {
        var result = await _sut.GetManufacturingKpisAsync(CancellationToken.None);

        result.TotalWorkOrders.Should().Be(0);
        result.TotalOrdered.Should().Be(0);
        result.TotalStocked.Should().Be(0);
        result.TotalScrapped.Should().Be(0);
        result.OverallYieldPct.Should().Be(0m);
        result.OverallScrapPct.Should().Be(0m);
    }

    [Fact]
    public async Task GetManufacturingKpisAsync_aggregates_quantities_and_calculates_percentages()
    {
        SeedWorkOrder(1, orderQty: 98, scrappedQty: 1);
        SeedWorkOrder(2, orderQty: 7, scrappedQty: 2);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var result = await _sut.GetManufacturingKpisAsync(CancellationToken.None);

        result.TotalWorkOrders.Should().Be(2);
        result.TotalOrdered.Should().Be(105);
        result.TotalScrapped.Should().Be(3);
        result.TotalStocked.Should().Be(102);
        result.OverallYieldPct.Should().Be(97.14m);
        result.OverallScrapPct.Should().Be(2.86m);
    }

    [Fact]
    public async Task GetManufacturingKpisAsync_with_no_scrap_returns_one_hundred_percent_yield()
    {
        SeedWorkOrder(1, orderQty: 10, scrappedQty: 0);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var result = await _sut.GetManufacturingKpisAsync(CancellationToken.None);

        result.TotalStocked.Should().Be(10);
        result.OverallYieldPct.Should().Be(100m);
        result.OverallScrapPct.Should().Be(0m);
    }

    [Fact]
    public async Task GetManufacturingKpisAsync_with_all_units_scrapped_returns_one_hundred_percent_scrap()
    {
        SeedWorkOrder(1, orderQty: 10, scrappedQty: 10);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var result = await _sut.GetManufacturingKpisAsync(CancellationToken.None);

        result.TotalStocked.Should().Be(0);
        result.OverallYieldPct.Should().Be(0m);
        result.OverallScrapPct.Should().Be(100m);
    }

    private void SeedWorkOrder(int workOrderId, int orderQty, short scrappedQty)
    {
        DbContext.WorkOrders.Add(new WorkOrder
        {
            WorkOrderId = workOrderId,
            ProductId = 747,
            OrderQty = orderQty,
            StockedQty = orderQty - scrappedQty,
            ScrappedQty = scrappedQty,
            StartDate = new DateTime(2011, 6, 1),
            DueDate = new DateTime(2011, 6, 14),
            ModifiedDate = StandardModifiedDate
        });
    }
}
