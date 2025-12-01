using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

[ExcludeFromCodeCoverage]
public sealed class SalesOrderRepositoryTests : PersistenceUnitTestBase
{
    private readonly SalesOrderRepository _sut;

    public SalesOrderRepositoryTests()
    {
        _sut = new SalesOrderRepository(DbContext);
    }

    [Fact]
    public async Task GetSalesOrdersAsync_returns_paginated_results()
    {
        // Arrange
        var customers = new[]
        {
            new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } },
            new CustomerEntity { CustomerId = 2, Person = new PersonEntity { BusinessEntityId = 2, FirstName = "Jane", LastName = "Smith" } }
        };

        var salesOrders = new[]
        {
            new SalesOrderHeader
            {
                SalesOrderId = 1,
                SalesOrderNumber = "SO1",
                OrderDate = new DateTime(2014, 1, 1),
                Status = 5,
                TotalDue = 1000m,
                CustomerId = 1,
                CustomerEntity = customers[0]
            },
            new SalesOrderHeader
            {
                SalesOrderId = 2,
                SalesOrderNumber = "SO2",
                OrderDate = new DateTime(2014, 1, 2),
                Status = 5,
                TotalDue = 2000m,
                CustomerId = 2,
                CustomerEntity = customers[1]
            }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, totalCount) = await _sut.GetSalesOrdersAsync(parameters, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        totalCount.Should().Be(2);
        results[0].SalesOrderNumber.Should().Be("SO1");
        results[1].SalesOrderNumber.Should().Be("SO2");
    }

    [Fact]
    public async Task GetSalesOrdersAsync_respects_pagination()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = Enumerable.Range(1, 25)
            .Select(i => new SalesOrderHeader
            {
                SalesOrderId = i,
                SalesOrderNumber = $"SO{i}",
                OrderDate = new DateTime(2014, 1, i),
                Status = 5,
                TotalDue = 1000 * i,
                CustomerId = 1,
                CustomerEntity = customer
            })
            .ToArray();

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 2, PageSize = 10 };

        // Act
        var (results, totalCount) = await _sut.GetSalesOrdersAsync(parameters, CancellationToken.None);

        // Assert
        results.Should().HaveCount(10);
        totalCount.Should().Be(25);
        results[0].SalesOrderNumber.Should().Be("SO11");
    }

    [Fact]
    public async Task SearchSalesOrdersAsync_filters_by_order_date_range()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = new[]
        {
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", OrderDate = new DateTime(2014, 3, 15), Status = 5, TotalDue = 2000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 3, SalesOrderNumber = "SO3", OrderDate = new DateTime(2014, 6, 30), Status = 5, TotalDue = 3000m, CustomerId = 1, CustomerEntity = customer }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel
        {
            OrderDateFrom = new DateTime(2014, 1, 1),
            OrderDateTo = new DateTime(2014, 6, 30)
        };

        // Act
        var (results, totalCount) = await _sut.SearchSalesOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchSalesOrdersAsync_filters_by_status()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = new[]
        {
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", OrderDate = new DateTime(2014, 1, 1), Status = 1, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", OrderDate = new DateTime(2014, 1, 2), Status = 5, TotalDue = 2000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 3, SalesOrderNumber = "SO3", OrderDate = new DateTime(2014, 1, 3), Status = 5, TotalDue = 3000m, CustomerId = 1, CustomerEntity = customer }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { Status = 5 };

        // Act
        var (results, totalCount) = await _sut.SearchSalesOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        totalCount.Should().Be(2);
        results.All(x => x.Status == 5).Should().BeTrue();
    }

    [Fact]
    public async Task SearchSalesOrdersAsync_filters_by_sales_person_id()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = new[]
        {
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", SalesPersonId = 1, OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", SalesPersonId = 2, OrderDate = new DateTime(2014, 1, 2), Status = 5, TotalDue = 2000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 3, SalesOrderNumber = "SO3", SalesPersonId = 1, OrderDate = new DateTime(2014, 1, 3), Status = 5, TotalDue = 3000m, CustomerId = 1, CustomerEntity = customer }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { SalesPersonId = 1 };

        // Act
        var (results, totalCount) = await _sut.SearchSalesOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        totalCount.Should().Be(2);
        results.All(x => x.SalesPersonId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task SearchSalesOrdersAsync_filters_by_territory_id()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = new[]
        {
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", TerritoryId = 1, OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", TerritoryId = 2, OrderDate = new DateTime(2014, 1, 2), Status = 5, TotalDue = 2000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 3, SalesOrderNumber = "SO3", TerritoryId = 1, OrderDate = new DateTime(2014, 1, 3), Status = 5, TotalDue = 3000m, CustomerId = 1, CustomerEntity = customer }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { TerritoryId = 1 };

        // Act
        var (results, totalCount) = await _sut.SearchSalesOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        totalCount.Should().Be(2);
        results.All(x => x.TerritoryId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetSalesOrdersAsync_uses_no_tracking()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };
        var salesOrder = new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer };

        DbContext.SalesOrderHeaders.Add(salesOrder);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, _) = await _sut.GetSalesOrdersAsync(parameters, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchSalesOrdersAsync_WithAccountNumber_ReturnsOnlyMatchingOrders()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = new[]
        {
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", AccountNumber = "10-4020-000676", OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", AccountNumber = "10-4020-000999", OrderDate = new DateTime(2014, 1, 2), Status = 5, TotalDue = 2000m, CustomerId = 1, CustomerEntity = customer }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { AccountNumber = "10-4020-000676" };

        // Act
        var (results, totalCount) = await _sut.SearchSalesOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        totalCount.Should().Be(1);
        results[0].AccountNumber.Should().Be("10-4020-000676");
    }

    [Fact]
    public async Task SearchSalesOrdersAsync_WithNullAccountNumber_ReturnsAllOrders()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrders = new[]
        {
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", AccountNumber = "10-4020-000676", OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 1000m, CustomerId = 1, CustomerEntity = customer },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", AccountNumber = "10-4020-000999", OrderDate = new DateTime(2014, 1, 2), Status = 5, TotalDue = 2000m, CustomerId = 1, CustomerEntity = customer }
        };

        DbContext.SalesOrderHeaders.AddRange(salesOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel { AccountNumber = null };

        // Act
        var (results, totalCount) = await _sut.SearchSalesOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetSalesOrderAnalyticsAsync_IsPartialMonth_IsFalse_WhenMaxOrderDate_IsLastDayOfMonth()
    {
        // Arrange — orders all in January 2014; max is the 31st (last day of month)
        DbContext.SalesOrderHeaders.AddRange(
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", OrderDate = new DateTime(2014, 1, 15), Status = 5, TotalDue = 500m, CustomerId = 1 },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", OrderDate = new DateTime(2014, 1, 31), Status = 5, TotalDue = 1000m, CustomerId = 1 }
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetSalesOrderAnalyticsAsync(null, CancellationToken.None);

        // Assert
        result.MonthlyTrend.Should().HaveCount(1);
        result.MonthlyTrend[0].IsPartialMonth.Should().BeFalse();
    }

    [Fact]
    public async Task GetSalesOrderAnalyticsAsync_IsPartialMonth_IsTrue_WhenMaxOrderDate_IsNotLastDayOfMonth()
    {
        // Arrange — orders in January 2014; max is the 15th (not last day of month)
        DbContext.SalesOrderHeaders.AddRange(
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", OrderDate = new DateTime(2014, 1, 1), Status = 5, TotalDue = 500m, CustomerId = 1 },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", OrderDate = new DateTime(2014, 1, 15), Status = 5, TotalDue = 1000m, CustomerId = 1 }
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetSalesOrderAnalyticsAsync(null, CancellationToken.None);

        // Assert
        result.MonthlyTrend.Should().HaveCount(1);
        result.MonthlyTrend[0].IsPartialMonth.Should().BeTrue();
    }

    [Fact]
    public async Task GetSalesOrderAnalyticsAsync_IsPartialMonth_OnlySetOnMostRecentMonthEntry()
    {
        // Arrange — three months; max order date is Feb 10 (not last day), so only Feb entry is partial
        DbContext.SalesOrderHeaders.AddRange(
            new SalesOrderHeader { SalesOrderId = 1, SalesOrderNumber = "SO1", OrderDate = new DateTime(2014, 1, 31), Status = 5, TotalDue = 100m, CustomerId = 1 },
            new SalesOrderHeader { SalesOrderId = 2, SalesOrderNumber = "SO2", OrderDate = new DateTime(2014, 2, 10), Status = 5, TotalDue = 200m, CustomerId = 1 }
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetSalesOrderAnalyticsAsync(null, CancellationToken.None);

        // Assert
        result.MonthlyTrend.Should().HaveCount(2);
        var jan = result.MonthlyTrend.Single(m => m.Month == 1);
        var feb = result.MonthlyTrend.Single(m => m.Month == 2);
        jan.IsPartialMonth.Should().BeFalse();
        feb.IsPartialMonth.Should().BeTrue();
    }

    [Fact]
    public async Task GetSalesOrderAnalyticsAsync_EarlyReturn_ReturnsEmptyTrendWithNoException()
    {
        // Arrange — filter that matches nothing; the early-return guard bypasses all aggregate queries

        // Act
        var result = await _sut.GetSalesOrderAnalyticsAsync(
            new SalesOrderSearchModel { SalesPersonId = 99999 },
            CancellationToken.None);

        // Assert
        result.MonthlyTrend.Should().BeEmpty();
        result.OrderCount.Should().Be(0);
    }

    [Fact]
    public async Task GetSalesOrderAnalyticsAsync_Take24_KeepsNewestMonths_AndSetsIsPartialMonthOnLast()
    {
        // Arrange — seed 26 distinct year/month groups (2012-01 through 2014-02).
        // Take(24) should keep the 24 newest (2012-03 through 2014-02), dropping 2012-01 and 2012-02.
        // Max order date is 2014-02-10 (not the last day of February), so 2014-02 is partial.
        var orders = new List<SalesOrderHeader>();
        int id = 1;
        for (int year = 2012; year <= 2014; year++)
        {
            int startMonth = (year == 2012) ? 1 : 1;
            int endMonth   = (year == 2014) ? 2 : 12;
            for (int month = startMonth; month <= endMonth; month++)
            {
                // Use the 10th of the month for all except the very last entry (2014-02)
                // which also gets the 10th — not the last day of February — so it is partial.
                orders.Add(new SalesOrderHeader
                {
                    SalesOrderId   = id++,
                    SalesOrderNumber = $"SO{id}",
                    OrderDate      = new DateTime(year, month, 10),
                    Status         = 5,
                    TotalDue       = 100m,
                    CustomerId     = 1
                });
            }
        }

        DbContext.SalesOrderHeaders.AddRange(orders);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetSalesOrderAnalyticsAsync(null, CancellationToken.None);

        // Assert — exactly 24 entries are returned
        result.MonthlyTrend.Should().HaveCount(24);

        // The oldest entry in the result must be 2012-03 (2012-01 and 2012-02 were dropped)
        result.MonthlyTrend[0].Year.Should().Be(2012);
        result.MonthlyTrend[0].Month.Should().Be(3);

        // The newest entry must be 2014-02
        var last = result.MonthlyTrend[^1];
        last.Year.Should().Be(2014);
        last.Month.Should().Be(2);

        // 2014-02-10 is not the last day of February, so IsPartialMonth must be true on the last entry
        last.IsPartialMonth.Should().BeTrue();

        // Every other entry must have IsPartialMonth = false
        result.MonthlyTrend.SkipLast(1).Should().AllSatisfy(m => m.IsPartialMonth.Should().BeFalse());
    }
}
