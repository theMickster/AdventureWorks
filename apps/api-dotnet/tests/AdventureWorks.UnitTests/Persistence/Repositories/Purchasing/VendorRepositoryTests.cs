using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Purchasing;

[ExcludeFromCodeCoverage]
public sealed class VendorRepositoryTests : PersistenceUnitTestBase
{
    private readonly VendorRepository _sut;

    public VendorRepositoryTests()
    {
        _sut = new VendorRepository(DbContext);
    }

    /// <summary>
    /// Pages through <see cref="VendorRepository.GetVendorsAsync"/> at the maximum allowed page
    /// size (50, per <c>VendorParameter</c>'s clamp) and concatenates every page, so tests can
    /// assert against a vendor population larger than a single page without special-casing the
    /// production page-size cap.
    /// </summary>
    private async Task<(List<VendorModel> Results, int TotalCount)> GetAllVendorsAsync(
        byte? creditRating = null,
        bool? preferredVendorStatus = null,
        bool? activeFlag = null)
    {
        const int maxAllowedPageSize = 50;
        var allResults = new List<VendorModel>();
        var pageNumber = 1;
        int totalCount;

        while (true)
        {
            var parameters = new VendorParameter
            {
                PageNumber = pageNumber,
                PageSize = maxAllowedPageSize,
                CreditRating = creditRating,
                PreferredVendorStatus = preferredVendorStatus,
                ActiveFlag = activeFlag
            };

            var (pageResults, pageTotalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);
            totalCount = pageTotalCount;
            allResults.AddRange(pageResults);

            if (allResults.Count >= totalCount || pageResults.Count == 0)
            {
                break;
            }

            pageNumber++;
        }

        return (allResults, totalCount);
    }

    /// <summary>
    /// Seeds a vendor (with its required <see cref="BusinessEntity"/> parent row) and, if
    /// <paramref name="spend"/> is provided, one purchase order per vendor totalling that spend.
    /// A vendor with a null <paramref name="spend"/> has zero purchase orders.
    /// </summary>
    private void SeedVendor(
        int vendorId,
        decimal? spend,
        byte creditRating = 1,
        bool preferredVendorStatus = true,
        bool activeFlag = true)
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
            CreditRating = creditRating,
            PreferredVendorStatus = preferredVendorStatus,
            ActiveFlag = activeFlag,
            PurchasingWebServiceUrl = string.Empty,
            ModifiedDate = StandardModifiedDate
        });

        if (spend.HasValue)
        {
            DbContext.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
            {
                PurchaseOrderId = vendorId,
                RevisionNumber = 1,
                Status = 4,
                EmployeeId = 1,
                VendorId = vendorId,
                ShipMethodId = 1,
                OrderDate = new DateTime(2014, 1, 1),
                SubTotal = spend.Value,
                TaxAmt = 0m,
                Freight = 0m,
                TotalDue = spend.Value,
                ModifiedDate = StandardModifiedDate
            });
        }
    }

    /// <summary>
    /// Seeds the full 104-vendor population (matching the real AdventureWorks vendor count) with a
    /// tie at the rank-52/53 boundary: vendor 52 and vendor 53 share the same spend, so both must
    /// receive rank 52 under true SQL <c>RANK()</c> semantics, and the vendor immediately after the
    /// tie must receive rank 54 (not 53) — proving ties are not silently collapsed to
    /// <c>ROW_NUMBER()</c>-style sequential ranks.
    /// </summary>
    /// <remarks>
    /// Vendor N gets spend (10000 - N * 10), for N = 1..51 (51 distinct, strictly descending
    /// values, ranks 1-51). Vendors 52 and 53 are tied at spend 9000 (both rank 52). Vendors
    /// 54-104 (51 vendors) descend further from there (ranks 54-104). Vendor 104 is seeded with
    /// zero purchase orders (TotalSpend = 0) to also cover the zero-PO case within the full
    /// population.
    /// </remarks>
    private void SeedFullVendorPopulation()
    {
        for (var vendorId = 1; vendorId <= 51; vendorId++)
        {
            SeedVendor(vendorId, spend: 10000m - vendorId * 10m, creditRating: 4);
        }

        // Tied pair: both rank 52.
        SeedVendor(52, spend: 9000m, creditRating: 4);
        SeedVendor(53, spend: 9000m, creditRating: 4);

        for (var vendorId = 54; vendorId <= 103; vendorId++)
        {
            SeedVendor(vendorId, spend: 8000m - vendorId * 10m, creditRating: 4);
        }

        // Vendor 104: zero purchase orders — lowest possible spend (0), rank 104.
        SeedVendor(104, spend: null, creditRating: 4);
    }

    [Fact]
    public async Task GetVendorsAsync_ComputesTrueRankSemantics_TiedVendorsShareRank_AndNextRankSkips()
    {
        // Arrange
        SeedFullVendorPopulation();
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act — page through at the max allowed page size (50) to observe the full population.
        var (results, totalCount) = await GetAllVendorsAsync();

        // Assert
        totalCount.Should().Be(104);
        results.Should().HaveCount(104);

        // Vendor 51 (rank 51) is high-risk: CreditRating 4 and rank <= 52.
        results.Single(x => x.VendorId == 51).IsHighRisk.Should().BeTrue();

        // Vendors 52 and 53 are tied at spend 9000 — both must be rank 52 (both high-risk).
        results.Single(x => x.VendorId == 52).IsHighRisk.Should().BeTrue();
        results.Single(x => x.VendorId == 53).IsHighRisk.Should().BeTrue();

        // Vendor 54 has the next distinct (lower) spend after the tie. Under true RANK()
        // semantics its rank is 54 (1 + 53 vendors with strictly greater spend), not 53 — so it
        // must NOT be high-risk, since 54 > 52.
        results.Single(x => x.VendorId == 54).IsHighRisk.Should().BeFalse();
    }

    [Fact]
    public async Task GetVendorsAsync_ZeroPoVendor_HasZeroSpendAndZeroPoCount_AndIsIncluded()
    {
        // Arrange
        SeedFullVendorPopulation();
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, _) = await GetAllVendorsAsync();

        // Assert — vendor 104 has no purchase orders, so it must still be present with zeros.
        var zeroPoVendor = results.Single(x => x.VendorId == 104);
        zeroPoVendor.TotalSpend.Should().Be(0m);
        zeroPoVendor.PoCount.Should().Be(0);
        zeroPoVendor.IsHighRisk.Should().BeFalse();
    }

    [Fact]
    public async Task GetVendorsAsync_OrdersByTotalSpendDescending()
    {
        // Arrange
        SeedVendor(1, spend: 100m);
        SeedVendor(2, spend: 300m);
        SeedVendor(3, spend: 200m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        totalCount.Should().Be(3);
        results.Select(x => x.VendorId).Should().ContainInOrder(2, 3, 1);
    }

    [Fact]
    public async Task GetVendorsAsync_PageBeyondTotal_ReturnsEmptyListWithCorrectTotalCount()
    {
        // Arrange
        SeedVendor(1, spend: 100m);
        SeedVendor(2, spend: 200m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 5, PageSize = 10 };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        results.Should().BeEmpty();
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetVendorsAsync_FiltersByCreditRating()
    {
        // Arrange
        SeedVendor(1, spend: 100m, creditRating: 1);
        SeedVendor(2, spend: 200m, creditRating: 4);
        SeedVendor(3, spend: 300m, creditRating: 4);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 1, PageSize = 10, CreditRating = 4 };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        totalCount.Should().Be(2);
        results.Select(x => x.VendorId).Should().BeEquivalentTo(new[] { 2, 3 });
    }

    [Fact]
    public async Task GetVendorsAsync_FiltersByPreferredVendorStatus()
    {
        // Arrange
        SeedVendor(1, spend: 100m, preferredVendorStatus: true);
        SeedVendor(2, spend: 200m, preferredVendorStatus: false);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 1, PageSize = 10, PreferredVendorStatus = false };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        totalCount.Should().Be(1);
        results[0].VendorId.Should().Be(2);
    }

    [Fact]
    public async Task GetVendorsAsync_FiltersByActiveFlag()
    {
        // Arrange
        SeedVendor(1, spend: 100m, activeFlag: true);
        SeedVendor(2, spend: 200m, activeFlag: false);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 1, PageSize = 10, ActiveFlag = false };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        totalCount.Should().Be(1);
        results[0].VendorId.Should().Be(2);
    }

    [Fact]
    public async Task GetVendorsAsync_CombinedFilters_ReturnsOnlyMatchingVendors()
    {
        // Arrange
        SeedVendor(1, spend: 100m, creditRating: 4, preferredVendorStatus: true, activeFlag: true);
        SeedVendor(2, spend: 200m, creditRating: 4, preferredVendorStatus: false, activeFlag: true);
        SeedVendor(3, spend: 300m, creditRating: 1, preferredVendorStatus: true, activeFlag: true);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter
        {
            PageNumber = 1,
            PageSize = 10,
            CreditRating = 4,
            PreferredVendorStatus = true,
            ActiveFlag = true
        };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        totalCount.Should().Be(1);
        results[0].VendorId.Should().Be(1);
    }

    [Fact]
    public async Task GetVendorsAsync_RankIsComputedBeforeFilters_NotAfter()
    {
        // Arrange — Regression test for the critical invariant: filtering to a subset must not
        // change any vendor's IsHighRisk value. Seed 40 CreditRating=1 vendors at HIGH spend
        // (occupying spend-ranks 1-40 in the full 55-vendor population, but excluded from
        // high-risk results by the CreditRating=4 filter since CreditRating=1 doesn't qualify),
        // plus 15 CreditRating=4 vendors at LOW spend (occupying spend-ranks 41-55: ranks 41-52
        // fall within the top-52 high-risk threshold, ranks 53-55 don't). This shape makes the
        // rank-before-filter invariant observable: if rank were (incorrectly) computed only over
        // the post-filter subset of 15 CreditRating=4 vendors, all 15 would rank 1-15 among
        // themselves and all would incorrectly qualify as high-risk (top-52 of 15). Because rank
        // is correctly computed pre-filter over the full 55-vendor population, only ranks 41-52
        // (12 vendors) are high-risk, not all 15.
        for (var vendorId = 1; vendorId <= 40; vendorId++)
        {
            // High spend, low credit rating — occupies ranks 1-40, filtered OUT by CreditRating=4.
            SeedVendor(vendorId, spend: 100000m - vendorId, creditRating: 1);
        }

        for (var vendorId = 41; vendorId <= 55; vendorId++)
        {
            // Lower spend, high credit rating — occupies ranks 41-55 in the FULL population.
            // Ranks 41-52 are high-risk; ranks 53-55 are not.
            SeedVendor(vendorId, spend: 1000m - vendorId, creditRating: 4);
        }

        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act — filter to only the CreditRating=4 vendors.
        var (filteredResults, filteredTotalCount) = await GetAllVendorsAsync(creditRating: 4);

        // Assert — even though only 15 vendors remain after filtering (which would put all of them
        // within a naive "top 52 of the filtered set"), their IsHighRisk values must still reflect
        // their rank in the FULL 55-vendor population: ranks 41-52 (12 vendors) are high-risk;
        // ranks 53-55 (3 vendors) are not, because rank was computed before the filter was applied.
        filteredTotalCount.Should().Be(15);
        filteredResults.Where(x => x.IsHighRisk).Should().HaveCount(12);
        filteredResults.Where(x => !x.IsHighRisk).Should().HaveCount(3);

        // The lowest-spend vendors (54, 55) in the full population fall outside rank 52 and must
        // not be high-risk despite the filter narrowing the visible set to only 15 vendors.
        filteredResults.Single(x => x.VendorId == 54).IsHighRisk.Should().BeFalse();
        filteredResults.Single(x => x.VendorId == 55).IsHighRisk.Should().BeFalse();

        // A vendor at the top of the CreditRating=4 subset (rank 41 overall) must be high-risk.
        filteredResults.Single(x => x.VendorId == 41).IsHighRisk.Should().BeTrue();
    }

    [Fact]
    public async Task GetVendorsAsync_IsHighRisk_False_WhenCreditRatingBelowThreshold_EvenIfTopRank()
    {
        // Arrange — vendor 1 has the highest spend (rank 1) but a low credit rating; must not be
        // high-risk since IsHighRisk requires CreditRating >= 4 AND rank <= 52.
        SeedVendor(1, spend: 1000m, creditRating: 3);
        SeedVendor(2, spend: 500m, creditRating: 4);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, _) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        results.Single(x => x.VendorId == 1).IsHighRisk.Should().BeFalse();
        results.Single(x => x.VendorId == 2).IsHighRisk.Should().BeTrue();
    }

    [Fact]
    public async Task GetVendorsAsync_CreditRatingLabel_MapsByteCodeToLabel()
    {
        // Arrange
        SeedVendor(1, spend: 100m, creditRating: 1);
        SeedVendor(2, spend: 90m, creditRating: 2);
        SeedVendor(3, spend: 80m, creditRating: 3);
        SeedVendor(4, spend: 70m, creditRating: 4);
        SeedVendor(5, spend: 60m, creditRating: 5);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, _) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        results.Single(x => x.VendorId == 1).CreditRatingLabel.Should().Be("Superior");
        results.Single(x => x.VendorId == 2).CreditRatingLabel.Should().Be("Excellent");
        results.Single(x => x.VendorId == 3).CreditRatingLabel.Should().Be("Above Average");
        results.Single(x => x.VendorId == 4).CreditRatingLabel.Should().Be("Average");
        results.Single(x => x.VendorId == 5).CreditRatingLabel.Should().Be("Below Average");
    }

    [Fact]
    public async Task GetVendorsAsync_RespectsPagination()
    {
        // Arrange
        for (var vendorId = 1; vendorId <= 10; vendorId++)
        {
            SeedVendor(vendorId, spend: (11 - vendorId) * 100m);
        }

        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var parameters = new VendorParameter { PageNumber = 2, PageSize = 4 };

        // Act
        var (results, totalCount) = await _sut.GetVendorsAsync(parameters, CancellationToken.None);

        // Assert
        totalCount.Should().Be(10);
        results.Should().HaveCount(4);
        results.Select(x => x.VendorId).Should().ContainInOrder(5, 6, 7, 8);
    }

    /// <summary>
    /// Seeds a single purchase order header with an explicit status/order date/total, so PO-history
    /// tests can build a specific shape without going through the aggregate-spend helper above.
    /// Callers must seed the vendor's <see cref="BusinessEntity"/>/<see cref="Vendor"/> parents
    /// separately (e.g. via <see cref="SeedVendor"/>) before calling this.
    /// </summary>
    private void SeedPurchaseOrder(
        int purchaseOrderId,
        int vendorId,
        byte status,
        DateTime orderDate,
        decimal totalDue = 100m)
    {
        DbContext.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
        {
            PurchaseOrderId = purchaseOrderId,
            RevisionNumber = 1,
            Status = status,
            EmployeeId = 1,
            VendorId = vendorId,
            ShipMethodId = 1,
            OrderDate = orderDate,
            SubTotal = totalDue,
            TaxAmt = 0m,
            Freight = 0m,
            TotalDue = totalDue,
            ModifiedDate = StandardModifiedDate
        });
    }

    private void SeedPurchaseOrderDetail(int purchaseOrderId, int purchaseOrderDetailId, DateTime dueDate)
    {
        DbContext.Add(new PurchaseOrderDetail
        {
            PurchaseOrderId = purchaseOrderId,
            PurchaseOrderDetailId = purchaseOrderDetailId,
            DueDate = dueDate,
            OrderQty = 1,
            ProductId = 1,
            UnitPrice = 10m,
            LineTotal = 10m,
            ReceivedQty = 0m,
            RejectedQty = 0m,
            StockedQty = 0m,
            ModifiedDate = StandardModifiedDate
        });
    }

    [Fact]
    public async Task GetVendorDetailAsync_UnknownVendor_ReturnsNull()
    {
        // Act
        var result = await _sut.GetVendorDetailAsync(9999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetVendorDetailAsync_ZeroPoVendor_ReturnsZeroSpendZeroCountZeroAverage()
    {
        // Arrange — real AdventureWorks zero-PO vendor shape (e.g. "Cycling Master", id 1502).
        SeedVendor(1502, spend: null, creditRating: 1);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetVendorDetailAsync(1502, TestContext.Current.CancellationToken);

        // Assert — guards the AvgPoValue divide-by-zero case: 0, not NaN/exception.
        result.Should().NotBeNull();
        result!.TotalSpend.Should().Be(0m);
        result.PoCount.Should().Be(0);
        result.AvgPoValue.Should().Be(0m);
    }

    [Fact]
    public async Task GetVendorDetailAsync_VendorWithPurchaseOrders_ComputesSpendMetrics()
    {
        // Arrange — real AdventureWorks vendor shape ("Advanced Bicycles", id 1496).
        DbContext.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = 1496,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });
        DbContext.Vendors.Add(new Vendor
        {
            BusinessEntityId = 1496,
            Name = "Advanced Bicycles",
            AccountNumber = "ADVANCED0001",
            CreditRating = 1,
            PreferredVendorStatus = true,
            ActiveFlag = true,
            PurchasingWebServiceUrl = string.Empty,
            ModifiedDate = StandardModifiedDate
        });
        SeedPurchaseOrder(3932, 1496, status: 1, orderDate: new DateTime(2014, 7, 30), totalDue: 302.44m);
        SeedPurchaseOrder(3853, 1496, status: 1, orderDate: new DateTime(2014, 7, 25), totalDue: 460.50m);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetVendorDetailAsync(1496, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Advanced Bicycles");
        result.CreditRatingLabel.Should().Be("Superior");
        result.PoCount.Should().Be(2);
        result.TotalSpend.Should().Be(762.94m);
        result.AvgPoValue.Should().Be(381.47m);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_UnknownVendor_ReturnsVendorExistsFalse()
    {
        // Act
        var (results, totalCount, vendorExists) = await _sut.GetVendorPurchaseOrdersAsync(
            9999, new VendorPurchaseOrderParameter { PageNumber = 1 }, TestContext.Current.CancellationToken);

        // Assert
        vendorExists.Should().BeFalse();
        results.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_ZeroPoVendor_ReturnsVendorExistsTrueWithEmptyPage()
    {
        // Arrange
        SeedVendor(1502, spend: null);
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, totalCount, vendorExists) = await _sut.GetVendorPurchaseOrdersAsync(
            1502, new VendorPurchaseOrderParameter { PageNumber = 1 }, TestContext.Current.CancellationToken);

        // Assert
        vendorExists.Should().BeTrue();
        results.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_DueDate_IsMinimumOfLineItemDueDates()
    {
        // Arrange — regression test for the DueDate-as-MIN decision: PurchaseOrderHeader has no
        // DueDate column, so the PO's due date must be derived as the earliest line-item DueDate.
        SeedVendor(1496, spend: null);
        SeedPurchaseOrder(3932, 1496, status: 1, orderDate: new DateTime(2014, 7, 30));
        SeedPurchaseOrderDetail(3932, 1, dueDate: new DateTime(2014, 8, 20));
        SeedPurchaseOrderDetail(3932, 2, dueDate: new DateTime(2014, 8, 13));
        SeedPurchaseOrderDetail(3932, 3, dueDate: new DateTime(2014, 8, 27));
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, _, _) = await _sut.GetVendorPurchaseOrdersAsync(
            1496, new VendorPurchaseOrderParameter { PageNumber = 1 }, TestContext.Current.CancellationToken);

        // Assert — the earliest of the three due dates (8/13) wins, not the first-seeded or last-seeded row.
        results.Should().ContainSingle();
        results[0].DueDate.Should().Be(new DateTime(2014, 8, 13));
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_FiltersByStatus()
    {
        // Arrange
        SeedVendor(1496, spend: null);
        SeedPurchaseOrder(1, 1496, status: 1, orderDate: new DateTime(2014, 1, 1));
        SeedPurchaseOrder(2, 1496, status: 4, orderDate: new DateTime(2014, 1, 2));
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, totalCount, _) = await _sut.GetVendorPurchaseOrdersAsync(
            1496, new VendorPurchaseOrderParameter { PageNumber = 1, Status = 4 }, TestContext.Current.CancellationToken);

        // Assert
        totalCount.Should().Be(1);
        results.Single().PurchaseOrderId.Should().Be(2);
        results.Single().StatusLabel.Should().Be("Complete");
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_FiltersByDateRange()
    {
        // Arrange
        SeedVendor(1496, spend: null);
        SeedPurchaseOrder(1, 1496, status: 1, orderDate: new DateTime(2014, 1, 1));
        SeedPurchaseOrder(2, 1496, status: 1, orderDate: new DateTime(2014, 6, 15));
        SeedPurchaseOrder(3, 1496, status: 1, orderDate: new DateTime(2014, 12, 31));
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, totalCount, _) = await _sut.GetVendorPurchaseOrdersAsync(
            1496,
            new VendorPurchaseOrderParameter { PageNumber = 1, StartDate = new DateTime(2014, 3, 1), EndDate = new DateTime(2014, 9, 1) },
            TestContext.Current.CancellationToken);

        // Assert
        totalCount.Should().Be(1);
        results.Single().PurchaseOrderId.Should().Be(2);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_OrdersByOrderDateDescending()
    {
        // Arrange
        SeedVendor(1496, spend: null);
        SeedPurchaseOrder(1, 1496, status: 1, orderDate: new DateTime(2014, 1, 1));
        SeedPurchaseOrder(2, 1496, status: 1, orderDate: new DateTime(2014, 6, 15));
        SeedPurchaseOrder(3, 1496, status: 1, orderDate: new DateTime(2014, 3, 1));
        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, _, _) = await _sut.GetVendorPurchaseOrdersAsync(
            1496, new VendorPurchaseOrderParameter { PageNumber = 1 }, TestContext.Current.CancellationToken);

        // Assert
        results.Select(x => x.PurchaseOrderId).Should().ContainInOrder(2, 3, 1);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_RespectsPagination()
    {
        // Arrange
        SeedVendor(1496, spend: null);
        for (var i = 1; i <= 5; i++)
        {
            SeedPurchaseOrder(i, 1496, status: 1, orderDate: new DateTime(2014, 1, i));
        }

        await DbContext.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var (results, totalCount, _) = await _sut.GetVendorPurchaseOrdersAsync(
            1496, new VendorPurchaseOrderParameter { PageNumber = 2, PageSize = 2 }, TestContext.Current.CancellationToken);

        // Assert — page 2 of 2, ordered by OrderDate desc: [5,4] page1, [3,2] page2.
        totalCount.Should().Be(5);
        results.Select(x => x.PurchaseOrderId).Should().ContainInOrder(3, 2);
    }
}
