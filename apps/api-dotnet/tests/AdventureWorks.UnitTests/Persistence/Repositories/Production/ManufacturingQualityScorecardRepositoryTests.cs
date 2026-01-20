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
    public async Task GetQualityScorecardAsync_with_work_orders_but_no_scrap_returns_all_empty_collections()
    {
        SeedProduct(2001, "Good Product");
        SeedWorkOrder(1, 2001, orderQty: 10, stockedQty: 10, scrappedQty: 0, scrapReasonId: null);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.Top5ByScrapped.Should().BeEmpty();
        result.Bottom5ByYield.Should().BeEmpty();
        result.ScrapReasonBreakdown.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQualityScorecardAsync_when_every_scrapped_row_has_a_reason_percentages_sum_to_100()
    {
        SeedProduct(3001, "Product A");
        SeedProduct(3002, "Product B");
        SeedProduct(3003, "Product C");
        SeedScrapReason(1, "Process issue");
        SeedScrapReason(2, "Handling damage");
        SeedScrapReason(3, "Tooling wear");
        SeedWorkOrder(1, 3001, orderQty: 100, stockedQty: 90, scrappedQty: 10, scrapReasonId: 1);
        SeedWorkOrder(2, 3002, orderQty: 100, stockedQty: 70, scrappedQty: 30, scrapReasonId: 2);
        SeedWorkOrder(3, 3003, orderQty: 100, stockedQty: 60, scrappedQty: 60, scrapReasonId: 3);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.ScrapReasonBreakdown.Select(reason => reason.ScrapReasonId)
            .Should().ContainInOrder((short)3, (short)2, (short)1);
        result.ScrapReasonBreakdown[0].ScrapReasonName.Should().Be("Tooling wear");
        result.ScrapReasonBreakdown.Select(reason => reason.ScrapPct)
            .Should().Equal(60m, 30m, 10m);
        result.ScrapReasonBreakdown.Sum(reason => reason.ScrapPct).Should().BeApproximately(100m, 0.05m);
    }

    [Fact]
    public async Task GetQualityScorecardAsync_scrap_without_reason_counts_in_totals_and_denominator_but_not_in_breakdown()
    {
        SeedProduct(4001, "Reasoned Product");
        SeedProduct(4002, "Unreasoned Product");
        SeedScrapReason(1, "Process issue");
        SeedWorkOrder(1, 4001, orderQty: 100, stockedQty: 70, scrappedQty: 30, scrapReasonId: 1);
        SeedWorkOrder(2, 4002, orderQty: 100, stockedQty: 90, scrappedQty: 10, scrapReasonId: null);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.Top5ByScrapped.Select(product => product.ProductId)
            .Should().ContainInOrder(4001, 4002);
        result.Top5ByScrapped[1].ScrappedQty.Should().Be(10);

        var reason = result.ScrapReasonBreakdown.Should().ContainSingle().Subject;
        reason.ScrapReasonId.Should().Be(1);
        reason.ScrappedQty.Should().Be(30);
        reason.ScrapPct.Should().Be(75m);
        result.ScrapReasonBreakdown.Sum(item => item.ScrapPct).Should().BeLessThan(100m);
    }

    [Fact]
    public async Task GetQualityScorecardAsync_top_scrapped_metrics_include_the_products_clean_work_orders()
    {
        SeedProduct(6001, "Mixed Product");
        SeedScrapReason(1, "Process issue");
        SeedWorkOrder(1, 6001, orderQty: 100, stockedQty: 90, scrappedQty: 10, scrapReasonId: 1);
        SeedWorkOrder(2, 6001, orderQty: 100, stockedQty: 100, scrappedQty: 0, scrapReasonId: null);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        var product = result.Top5ByScrapped.Should().ContainSingle().Subject;
        product.OrderedQty.Should().Be(200);
        product.StockedQty.Should().Be(190);
        product.ScrappedQty.Should().Be(10);
        product.YieldPct.Should().Be(95m);
        product.ScrapPct.Should().Be(5m);
    }

    [Fact]
    public async Task GetQualityScorecardAsync_bottom_yield_sorts_on_the_raw_ratio_not_the_rounded_percentage()
    {
        SeedProduct(5001, "Higher raw yield");
        SeedProduct(5002, "Lower raw yield");
        SeedScrapReason(1, "Process issue");
        SeedWorkOrder(1, 5001, orderQty: 30000, stockedQty: 20001, scrappedQty: 9999, scrapReasonId: 1);
        SeedWorkOrder(2, 5002, orderQty: 3, stockedQty: 2, scrappedQty: 1, scrapReasonId: 1);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _sut.GetQualityScorecardAsync(CancellationToken.None);

        result.Bottom5ByYield.Select(product => product.YieldPct).Should().Equal(66.67m, 66.67m);
        result.Bottom5ByYield.Select(product => product.ProductId).Should().ContainInOrder(5002, 5001);
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
