using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Purchasing;

[ExcludeFromCodeCoverage]
public sealed class PurchasingAnalyticsRepositoryTests : PersistenceUnitTestBase
{
    private readonly PurchasingAnalyticsRepository _sut;

    public PurchasingAnalyticsRepositoryTests()
    {
        _sut = new PurchasingAnalyticsRepository(DbContext);
    }

    /// <summary>
    /// Seeds a vendor and its required <see cref="BusinessEntity"/> parent row. Purchase orders are
    /// seeded separately via <see cref="SeedPurchaseOrder"/> so each test controls exactly how many
    /// purchase orders (and which statuses) a vendor has — including none at all.
    /// </summary>
    private void SeedVendor(int vendorId, bool activeFlag = true)
    {
        DbContext.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = vendorId,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.Vendors.Add(new Vendor
        {
            BusinessEntityId = vendorId,
            Name = $"Vendor {vendorId}",
            AccountNumber = $"ACCT{vendorId:D5}",
            CreditRating = 1,
            PreferredVendorStatus = true,
            ActiveFlag = activeFlag,
            PurchasingWebServiceUrl = string.Empty,
            ModifiedDate = StandardModifiedDate
        });
    }

    /// <summary>
    /// Seeds a single purchase order for a vendor with the supplied total and status.
    /// </summary>
    private void SeedPurchaseOrder(int purchaseOrderId, int vendorId, decimal totalDue, byte status = 4)
    {
        DbContext.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
        {
            PurchaseOrderId = purchaseOrderId,
            RevisionNumber = 1,
            Status = status,
            EmployeeId = 1,
            VendorId = vendorId,
            ShipMethodId = 1,
            OrderDate = new DateTime(2014, 1, 1),
            SubTotal = totalDue,
            TaxAmt = 0m,
            Freight = 0m,
            TotalDue = totalDue,
            ModifiedDate = StandardModifiedDate
        });
    }

    [Fact]
    public async Task GetPurchasingAnalyticsAsync_CumulativePercentIncreasesAndLastEntryIsExactly100()
    {
        // Arrange — three distinct, non-round spends so the running division cannot land on 100
        // by accident; 700/3 style repeating decimals are exactly the case that leaves the final
        // entry a hair off 100 unless it is pinned.
        SeedVendor(1);
        SeedVendor(2);
        SeedVendor(3);
        SeedPurchaseOrder(101, vendorId: 1, totalDue: 100m);
        SeedPurchaseOrder(102, vendorId: 2, totalDue: 300m);
        SeedPurchaseOrder(103, vendorId: 3, totalDue: 300m / 7m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchasingAnalyticsAsync(CancellationToken.None);

        // Assert
        result.ParetoData.Should().HaveCount(3);
        result.ParetoData.Select(x => x.CumulativePercent).Should().BeInAscendingOrder();
        result.ParetoData.Select(x => x.CumulativePercent).Should().OnlyHaveUniqueItems();
        result.ParetoData[^1].CumulativePercent.Should().Be(100m);
    }

    [Fact]
    public async Task GetPurchasingAnalyticsAsync_ReturnsAllFourStatuses_WhenAStatusHasZeroPurchaseOrders()
    {
        // Arrange — statuses 1 (Pending) and 4 (Complete) have purchase orders; 2 (Approved) and
        // 3 (Rejected) have none and must still appear as zero-value rows.
        SeedVendor(1);
        SeedPurchaseOrder(101, vendorId: 1, totalDue: 100m, status: 1);
        SeedPurchaseOrder(102, vendorId: 1, totalDue: 250m, status: 4);
        SeedPurchaseOrder(103, vendorId: 1, totalDue: 150m, status: 4);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchasingAnalyticsAsync(CancellationToken.None);

        // Assert — always four rows, in status-code order.
        result.PipelineSummary.Select(x => x.StatusLabel)
            .Should().ContainInOrder("Pending", "Approved", "Rejected", "Complete");

        var pending = result.PipelineSummary.Single(x => x.StatusLabel == "Pending");
        pending.PoCount.Should().Be(1);
        pending.TotalValue.Should().Be(100m);

        var complete = result.PipelineSummary.Single(x => x.StatusLabel == "Complete");
        complete.PoCount.Should().Be(2);
        complete.TotalValue.Should().Be(400m);

        var approved = result.PipelineSummary.Single(x => x.StatusLabel == "Approved");
        approved.PoCount.Should().Be(0);
        approved.TotalValue.Should().Be(0m);

        var rejected = result.PipelineSummary.Single(x => x.StatusLabel == "Rejected");
        rejected.PoCount.Should().Be(0);
        rejected.TotalValue.Should().Be(0m);
    }

    [Fact]
    public async Task GetPurchasingAnalyticsAsync_VendorWithZeroPurchaseOrders_IsStillIncludedWithZeroSpend()
    {
        // Arrange — vendor 2 has no purchase orders. It must not be dropped by the spend join.
        // It is also seeded inactive to prove no implicit ActiveFlag filter is applied.
        SeedVendor(1);
        SeedVendor(2, activeFlag: false);
        SeedPurchaseOrder(101, vendorId: 1, totalDue: 500m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchasingAnalyticsAsync(CancellationToken.None);

        // Assert
        result.ParetoData.Should().HaveCount(2);

        var zeroSpendVendor = result.ParetoData.Single(x => x.VendorId == 2);
        zeroSpendVendor.TotalSpend.Should().Be(0m);
        zeroSpendVendor.VendorName.Should().Be("Vendor 2");

        // The zero-spend vendor sorts last, so it carries the pinned 100.
        result.ParetoData[^1].VendorId.Should().Be(2);
    }

    [Fact]
    public async Task GetPurchasingAnalyticsAsync_VendorsWithIdenticalSpend_SortByVendorIdAscending()
    {
        // Arrange — vendors 7 and 3 are tied at 500. Seeded highest-id-first so an unordered
        // (insertion-order) result would put vendor 7 before vendor 3.
        SeedVendor(7);
        SeedVendor(3);
        SeedVendor(1);
        SeedPurchaseOrder(107, vendorId: 7, totalDue: 500m);
        SeedPurchaseOrder(103, vendorId: 3, totalDue: 500m);
        SeedPurchaseOrder(101, vendorId: 1, totalDue: 900m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchasingAnalyticsAsync(CancellationToken.None);

        // Assert — spend descending, then VendorId ascending within the tie.
        result.ParetoData.Select(x => x.VendorId).Should().ContainInOrder(1, 3, 7);
    }

    [Fact]
    public async Task GetPurchasingAnalyticsAsync_WithNoPurchaseOrdersAtAll_ReturnsAllVendorsAtZero_AndFourZeroStatuses()
    {
        // Arrange — vendors exist, zero purchase orders. Total spend is 0, so the cumulative
        // percentage must not divide by zero, and ParetoData must not be empty.
        SeedVendor(1);
        SeedVendor(2);
        SeedVendor(3);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchasingAnalyticsAsync(CancellationToken.None);

        // Assert
        result.ParetoData.Should().HaveCount(3);
        result.ParetoData.Should().OnlyContain(x => x.TotalSpend == 0m);
        result.ParetoData.Should().OnlyContain(x => x.CumulativePercent == 0m);
        result.ParetoData.Select(x => x.VendorId).Should().ContainInOrder(1, 2, 3);

        result.PipelineSummary.Should().HaveCount(4);
        result.PipelineSummary.Should().OnlyContain(x => x.PoCount == 0 && x.TotalValue == 0m);
        result.PipelineSummary.Select(x => x.StatusLabel)
            .Should().ContainInOrder("Pending", "Approved", "Rejected", "Complete");
    }

    [Fact]
    public async Task GetPurchasingAnalyticsAsync_SingleVendorWithPurchaseOrders_HasCumulativePercentOf100()
    {
        // Arrange
        SeedVendor(1);
        SeedPurchaseOrder(101, vendorId: 1, totalDue: 123.45m);
        SeedPurchaseOrder(102, vendorId: 1, totalDue: 76.55m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchasingAnalyticsAsync(CancellationToken.None);

        // Assert
        result.ParetoData.Should().HaveCount(1);
        result.ParetoData[0].VendorId.Should().Be(1);
        result.ParetoData[0].TotalSpend.Should().Be(200m);
        result.ParetoData[0].CumulativePercent.Should().Be(100m);
    }
}
