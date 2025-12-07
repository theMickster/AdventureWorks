using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

[ExcludeFromCodeCoverage]
public sealed class CustomerRepositoryTests : PersistenceUnitTestBase
{
    private readonly CustomerRepository _sut;

    public CustomerRepositoryTests()
    {
        _sut = new CustomerRepository(DbContext);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(CustomerRepository)
                .Should().Implement<ICustomerRepository>();

            typeof(CustomerRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    private void SeedCustomers()
    {
        DbContext.Persons.AddRange(new List<PersonEntity>
        {
            new() { BusinessEntityId = 1, FirstName = "Jon", LastName = "Yang", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { BusinessEntityId = 2, FirstName = "No", LastName = "Orders", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { BusinessEntityId = 3, FirstName = "Old", LastName = "Customer", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { BusinessEntityId = 4, FirstName = "Tie", LastName = "Alpha", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { BusinessEntityId = 5, FirstName = "Tie", LastName = "Beta", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate }
        });

        DbContext.Stores.Add(new StoreEntity
        {
            BusinessEntityId = 900,
            Name = "Topnotch Bikes",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.Set<CustomerEntity>().AddRange(new List<CustomerEntity>
        {
            new() { CustomerId = 11000, PersonId = 1, AccountNumber = "AW00011000", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { CustomerId = 11001, PersonId = 2, AccountNumber = "AW00011001", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { CustomerId = 11002, PersonId = 3, AccountNumber = "AW00011002", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { CustomerId = 11003, StoreId = 900, AccountNumber = "AW00011003", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { CustomerId = 11004, PersonId = 4, AccountNumber = "AW00011004", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate },
            new() { CustomerId = 11005, PersonId = 5, AccountNumber = "AW00011005", Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate }
        });

        DbContext.SalesOrderHeaders.AddRange(new List<SalesOrderHeader>
        {
            new()
            {
                SalesOrderId = 1, CustomerId = 11000, SalesOrderNumber = "SO1", AccountNumber = "AW00011000",
                OrderDate = new DateTime(2026, 5, 1), DueDate = new DateTime(2026, 5, 8), TotalDue = 900m,
                Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate
            },
            new()
            {
                SalesOrderId = 2, CustomerId = 11002, SalesOrderNumber = "SO2", AccountNumber = "AW00011002",
                OrderDate = new DateTime(2024, 1, 1), DueDate = new DateTime(2024, 1, 8), TotalDue = 200m,
                Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate
            },
            new()
            {
                SalesOrderId = 3, CustomerId = 11003, SalesOrderNumber = "SO3", AccountNumber = "AW00011003",
                OrderDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 8), TotalDue = 500m,
                Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate
            },
            new()
            {
                SalesOrderId = 4, CustomerId = 11004, SalesOrderNumber = "SO4", AccountNumber = "AW00011004",
                OrderDate = new DateTime(2026, 2, 1), DueDate = new DateTime(2026, 2, 8), TotalDue = 0m,
                Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate
            },
            new()
            {
                SalesOrderId = 5, CustomerId = 11005, SalesOrderNumber = "SO5", AccountNumber = "AW00011005",
                OrderDate = new DateTime(2026, 2, 1), DueDate = new DateTime(2026, 2, 8), TotalDue = 0m,
                Rowguid = Guid.NewGuid(), ModifiedDate = StandardModifiedDate
            }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task GetCustomersAsync_ranks_by_totalSpend_descending_then_customerId_ascendingAsync()
    {
        SeedCustomers();

        var (results, totalCount) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 50 });

        using (new AssertionScope())
        {
            totalCount.Should().Be(6);
            results.Should().HaveCount(6);

            results[0].CustomerId.Should().Be(11000);
            results[0].TotalSpend.Should().Be(900m);
            results[0].LtvRank.Should().Be(1);
            results[0].CustomerType.Should().Be("Individual");
            results[0].DisplayName.Should().Be("Jon Yang");

            results[1].CustomerId.Should().Be(11003);
            results[1].TotalSpend.Should().Be(500m);
            results[1].LtvRank.Should().Be(2);
            results[1].CustomerType.Should().Be("Store");
            results[1].DisplayName.Should().Be("Topnotch Bikes");

            results[2].CustomerId.Should().Be(11002);
            results[2].TotalSpend.Should().Be(200m);
            results[2].LtvRank.Should().Be(3);
        }
    }

    [Fact]
    public async Task GetCustomersAsync_assigns_sequential_distinct_ranks_among_zero_dollar_tiesAsync()
    {
        SeedCustomers();

        var (results, _) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 50 });

        var zeroSpendCustomers = results.Where(r => r.TotalSpend == 0m).OrderBy(r => r.CustomerId).ToList();

        using (new AssertionScope())
        {
            // No orders (11001), and two $0-order customers (11004, 11005) tie at $0 spend.
            zeroSpendCustomers.Should().HaveCount(3);
            zeroSpendCustomers.Select(r => r.CustomerId).Should().BeInAscendingOrder();

            var ranks = zeroSpendCustomers.Select(r => r.LtvRank).ToList();
            ranks.Should().OnlyHaveUniqueItems("because ROW_NUMBER semantics never share ranks, even among ties");
            ranks.Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetCustomersAsync_marks_customer_with_no_orders_ever_as_inactiveAsync()
    {
        SeedCustomers();

        var (results, _) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 50 });

        var neverOrdered = results.Single(r => r.CustomerId == 11001);

        using (new AssertionScope())
        {
            neverOrdered.OrderCount.Should().Be(0);
            neverOrdered.TotalSpend.Should().Be(0m);
            neverOrdered.LastOrderDate.Should().BeNull();
            neverOrdered.IsInactive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetCustomersAsync_marks_customer_older_than_twelve_months_from_most_recent_order_as_inactiveAsync()
    {
        SeedCustomers();

        // Most recent order across the dataset is 2026-05-01 (customer 11000).
        // Customer 11002's last order is 2024-01-01, well past the 12-month cutoff.
        var (results, _) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 50 });

        var mostRecentCustomer = results.Single(r => r.CustomerId == 11000);
        var staleCustomer = results.Single(r => r.CustomerId == 11002);

        using (new AssertionScope())
        {
            mostRecentCustomer.IsInactive.Should().BeFalse();
            staleCustomer.IsInactive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetCustomersAsync_search_filters_by_displayName_and_preserves_global_rankAsync()
    {
        SeedCustomers();

        var (unfiltered, _) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 50 });
        var expectedRank = unfiltered.Single(r => r.CustomerId == 11002).LtvRank;

        var (results, totalCount) = await _sut.GetCustomersAsync(
            new CustomerParameter { PageNumber = 1, PageSize = 50, Search = "OLD" });

        using (new AssertionScope())
        {
            totalCount.Should().Be(1);
            results.Should().HaveCount(1);
            results[0].CustomerId.Should().Be(11002);
            results[0].LtvRank.Should().Be(expectedRank, "because rank is assigned before search filtering and must remain stable");
        }
    }

    [Fact]
    public async Task GetCustomersAsync_paginates_resultsAsync()
    {
        SeedCustomers();

        var (page1, totalCount) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 2 });
        var (page2, _) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 2, PageSize = 2 });

        using (new AssertionScope())
        {
            totalCount.Should().Be(6);
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
            page1[0].CustomerId.Should().NotBe(page2[0].CustomerId);
        }
    }

    [Fact]
    public async Task GetCustomersAsync_returns_empty_when_no_customers_existAsync()
    {
        var (results, totalCount) = await _sut.GetCustomersAsync(new CustomerParameter { PageNumber = 1, PageSize = 10 });

        using (new AssertionScope())
        {
            totalCount.Should().Be(0);
            results.Should().BeEmpty();
        }
    }
}
