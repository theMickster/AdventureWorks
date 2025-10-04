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
}
