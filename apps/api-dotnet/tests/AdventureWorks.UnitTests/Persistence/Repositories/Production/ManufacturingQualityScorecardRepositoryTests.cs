using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories.Production;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Production;

[ExcludeFromCodeCoverage]
public sealed class ManufacturingQualityScorecardRepositoryTests : PersistenceUnitTestBase
{
    private readonly ManufacturingQualityScorecardRepository _sut;

    public ManufacturingQualityScorecardRepositoryTests()
    {
        _sut = new ManufacturingQualityScorecardRepository(DbContext);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        typeof(ManufacturingQualityScorecardRepository)
            .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetQualityScorecardAsync_with_no_work_orders_returns_empty_collections()
    {
        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.Top5ByScrapped.Should().BeEmpty();
        result.Bottom5ByYield.Should().BeEmpty();
        result.ScrapReasonBreakdown.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQualityScorecardAsync_ranks_limits_and_calculates_product_metrics()
    {
        SeedProduct(1001, "Highest Scrap");
        SeedProduct(1002, "Second Scrap");
        SeedProduct(1003, "Third Scrap");
        SeedProduct(1004, "Fourth Scrap");
        SeedProduct(1005, "Fifth Scrap");
        SeedProduct(1006, "Sixth Scrap");
        SeedProduct(1007, "Zero Order");
        SeedScrapReason(1, "Process issue");
        SeedScrapReason(2, "Handling damage");

        SeedWorkOrder(1, 1001, orderQty: 100, stockedQty: 50, scrappedQty: 50, scrapReasonId: 1);
        SeedWorkOrder(2, 1002, orderQty: 100, stockedQty: 80, scrappedQty: 20, scrapReasonId: 1);
        SeedWorkOrder(3, 1003, orderQty: 100, stockedQty: 90, scrappedQty: 10, scrapReasonId: 2);
        SeedWorkOrder(4, 1004, orderQty: 100, stockedQty: 70, scrappedQty: 30, scrapReasonId: 2);
        SeedWorkOrder(5, 1005, orderQty: 100, stockedQty: 95, scrappedQty: 5, scrapReasonId: 1);
        SeedWorkOrder(6, 1006, orderQty: 100, stockedQty: 60, scrappedQty: 40, scrapReasonId: 2);
        SeedWorkOrder(7, 1007, orderQty: 0, stockedQty: 0, scrappedQty: 99, scrapReasonId: 2);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.Top5ByScrapped.Should().HaveCount(5);
        result.Top5ByScrapped.Select(product => product.ProductId)
            .Should().ContainInOrder(1007, 1001, 1006, 1004, 1002);
        result.Top5ByScrapped[0].ScrapPct.Should().Be(0m);
        result.Top5ByScrapped[1].ProductName.Should().Be("Highest Scrap");
        result.Top5ByScrapped[1].ScrapPct.Should().Be(50m);

        result.Bottom5ByYield.Should().HaveCount(5);
        result.Bottom5ByYield.Select(product => product.ProductId)
            .Should().ContainInOrder(1001, 1006, 1004, 1002, 1003);
        result.Bottom5ByYield.Should().NotContain(product => product.ProductId == 1007);
        result.Bottom5ByYield[0].YieldPct.Should().Be(50m);
        result.Bottom5ByYield[4].YieldPct.Should().Be(90m);
    }

    [Fact]
    public async Task GetQualityScorecardAsync_with_no_scrap_returns_empty_scrap_collections_and_yield_data()
    {
        SeedProduct(2001, "Good Product");
        SeedWorkOrder(1, 2001, orderQty: 10, stockedQty: 10, scrappedQty: 0, scrapReasonId: null);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.Top5ByScrapped.Should().BeEmpty();
        result.ScrapReasonBreakdown.Should().BeEmpty();
        result.Bottom5ByYield.Should().ContainSingle().Which.YieldPct.Should().Be(100m);
    }

    [Fact]
    public async Task GetQualityScorecardAsync_calculates_scrap_reason_percentages_from_categorized_scrap()
    {
        SeedProduct(3001, "Product A");
        SeedProduct(3002, "Product B");
        SeedScrapReason(1, "Process issue");
        SeedScrapReason(2, "Handling damage");
        SeedWorkOrder(1, 3001, orderQty: 100, stockedQty: 90, scrappedQty: 10, scrapReasonId: 1);
        SeedWorkOrder(2, 3002, orderQty: 100, stockedQty: 70, scrappedQty: 30, scrapReasonId: 2);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.ScrapReasonBreakdown.Should().HaveCount(2);
        result.ScrapReasonBreakdown[0].ScrapReasonId.Should().Be(2);
        result.ScrapReasonBreakdown[0].ScrapReasonName.Should().Be("Handling damage");
        result.ScrapReasonBreakdown[0].ScrapPct.Should().Be(75m);
        result.ScrapReasonBreakdown[1].ScrapPct.Should().Be(25m);
    }

    private void SeedProduct(int productId, string name)
    {
        DbContext.Products.Add(new Product
        {
            ProductId = productId,
            Name = name,
            ModifiedDate = StandardModifiedDate
        });
    }

    private void SeedScrapReason(short scrapReasonId, string name)
    {
        DbContext.ScrapReasons.Add(new ScrapReason
        {
            ScrapReasonId = scrapReasonId,
            Name = name,
            ModifiedDate = StandardModifiedDate
        });
    }

    private void SeedWorkOrder(
        int workOrderId,
        int productId,
        int orderQty,
        int stockedQty,
        short scrappedQty,
        short? scrapReasonId)
    {
        DbContext.WorkOrders.Add(new WorkOrder
        {
            WorkOrderId = workOrderId,
            ProductId = productId,
            OrderQty = orderQty,
            StockedQty = stockedQty,
            ScrappedQty = scrappedQty,
            ScrapReasonId = scrapReasonId,
            StartDate = new DateTime(2011, 6, 1),
            DueDate = new DateTime(2011, 6, 14),
            ModifiedDate = StandardModifiedDate
        });
    }
}
