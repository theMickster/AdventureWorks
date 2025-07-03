using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;
using AdventureWorks.UnitTests.Setup.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

[ExcludeFromCodeCoverage]
public sealed class SalesPersonRepositoryTests : PersistenceUnitTestBase
{
    private readonly SalesPersonRepository _sut;

    public SalesPersonRepositoryTests()
    {
        _sut = new SalesPersonRepository(DbContext);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(SalesPersonRepository)
                .Should().Implement<ISalesPersonRepository>();

            typeof(SalesPersonRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    private async Task SeedTerritoriesAndSalesPersons()
    {
        var territories = SalesDomainFixtures.GetSalesTerritories();
        DbContext.SalesTerritories.AddRange(territories);
        await DbContext.SaveChangesAsync();

        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();
    }

    private async Task AddTerritoryAsync(int territoryId, string name)
    {
        var territory = new SalesTerritoryEntity
        {
            TerritoryId = territoryId,
            Name = name,
            CountryRegionCode = "US",
            Group = "North America",
            SalesYtd = 0,
            SalesLastYear = 0,
            CostYtd = 0,
            CostLastYear = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = SalesDomainFixtures.SalesDomainDefaultAuditDate
        };
        DbContext.SalesTerritories.Add(territory);
        await DbContext.SaveChangesAsync();
    }

    #region GetSalesPersonByIdAsync Tests

    [Fact]
    public async Task GetSalesPersonByIdAsync_returns_sales_person_with_related_data()
    {
        await AddTerritoryAsync(1, "Northwest");

        var salesPerson = SalesDomainFixtures.GetCompleteSalesPersonEntity(
            100, "Test", "User", "Sales Rep", 1, "Northwest", "test.user@adventure-works.com");
        DbContext.SalesPersons.Add(salesPerson);
        await DbContext.SaveChangesAsync();

        var result = await _sut.GetSalesPersonByIdAsync(100);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(100);
            result.TerritoryId.Should().Be(1);
            result.Employee.Should().NotBeNull();
            result.Employee.PersonBusinessEntity.Should().NotBeNull();
            result.Employee.PersonBusinessEntity.FirstName.Should().Be("Test");
            result.Employee.PersonBusinessEntity.LastName.Should().Be("User");
            result.Employee.PersonBusinessEntity.EmailAddresses.Should().NotBeEmpty();
            result.SalesTerritory.Should().NotBeNull();
            result.SalesTerritory.Name.Should().Be("Northwest");
        }
    }

    [Fact]
    public async Task GetSalesPersonByIdAsync_returns_null_for_nonexistent_id()
    {
        var result = await _sut.GetSalesPersonByIdAsync(99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSalesPersonByIdAsync_uses_no_tracking()
    {
        var salesPerson = SalesDomainFixtures.GetCompleteSalesPersonEntity();
        DbContext.SalesPersons.Add(salesPerson);
        await DbContext.SaveChangesAsync();

        var result = await _sut.GetSalesPersonByIdAsync(100);

        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region GetSalesPersonsAsync Tests

    [Fact]
    public async Task GetSalesPersonsAsync_returns_paginated_sales_persons_with_total_count()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 3 };

        var (result, totalCount) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            totalCount.Should().Be(5);
            result.Should().AllSatisfy(sp =>
            {
                sp.Employee.Should().NotBeNull();
                sp.Employee.PersonBusinessEntity.Should().NotBeNull();
            });
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_honors_page_size_and_number()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 2, PageSize = 2 };

        var (result, totalCount) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            totalCount.Should().Be(5);
            result[0].BusinessEntityId.Should().Be(3);
            result[1].BusinessEntityId.Should().Be(4);
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_sorts_by_BusinessEntityId_ascending()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "id",
            SortOrder = SortedResultConstants.Ascending
        };

        var (result, _) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInAscendingOrder(sp => sp.BusinessEntityId);
            result[0].BusinessEntityId.Should().Be(1);
            result[^1].BusinessEntityId.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_sorts_by_BusinessEntityId_descending()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "id",
            SortOrder = SortedResultConstants.Descending
        };

        var (result, _) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInDescendingOrder(sp => sp.BusinessEntityId);
            result[0].BusinessEntityId.Should().Be(5);
            result[^1].BusinessEntityId.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_sorts_by_FirstName_ascending()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "firstName",
            SortOrder = SortedResultConstants.Ascending
        };

        var (result, _) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInAscendingOrder(sp => sp.Employee.PersonBusinessEntity.FirstName);
            result[0].Employee.PersonBusinessEntity.FirstName.Should().Be("Alice");
            result[^1].Employee.PersonBusinessEntity.FirstName.Should().Be("Edward");
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_sorts_by_LastName_descending()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "lastName",
            SortOrder = SortedResultConstants.Descending
        };

        var (result, _) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInDescendingOrder(sp => sp.Employee.PersonBusinessEntity.LastName);
            result[0].Employee.PersonBusinessEntity.LastName.Should().Be("Evans");
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_returns_empty_list_when_no_sales_persons()
    {
        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };

        var (result, totalCount) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_includes_employee_person_and_territory_data()
    {
        await AddTerritoryAsync(1, "Northwest");

        var salesPerson = SalesDomainFixtures.GetCompleteSalesPersonEntity(
            1, "Test", "User", "Sales Rep", 1, "Northwest", "test.user@adventure-works.com");
        DbContext.SalesPersons.Add(salesPerson);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };

        var (result, _) = await _sut.GetSalesPersonsAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Employee.Should().NotBeNull();
            result[0].Employee.PersonBusinessEntity.Should().NotBeNull();
            result[0].Employee.PersonBusinessEntity.FirstName.Should().Be("Test");
            result[0].Employee.PersonBusinessEntity.LastName.Should().Be("User");
            result[0].Employee.PersonBusinessEntity.EmailAddresses.Should().NotBeEmpty();
            result[0].SalesTerritory.Should().NotBeNull();
            result[0].SalesTerritory.Name.Should().Be("Northwest");
        }
    }

    [Fact]
    public async Task GetSalesPersonsAsync_uses_no_tracking()
    {
        var salesPerson = SalesDomainFixtures.GetCompleteSalesPersonEntity();
        DbContext.SalesPersons.Add(salesPerson);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };

        var (result, _) = await _sut.GetSalesPersonsAsync(parameters);

        var entry = DbContext.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region SearchSalesPersonsAsync Tests

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_Id()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { Id = 3 };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].BusinessEntityId.Should().Be(3);
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_FirstName()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { FirstName = "bob" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Employee.PersonBusinessEntity.FirstName.Should().Be("Bob");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_LastName()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { LastName = "davis" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Employee.PersonBusinessEntity.LastName.Should().Be("Davis");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_SalesTerritoryId()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { SalesTerritoryId = 1 };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(sp => sp.TerritoryId.Should().Be(1));
            totalCount.Should().Be(2);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_SalesTerritoryName()
    {
        await SeedTerritoriesAndSalesPersons();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { SalesTerritoryName = "northeast" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].TerritoryId.Should().Be(2);
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_SalesTerritoryGroupName()
    {
        await SeedTerritoriesAndSalesPersons();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { SalesTerritoryGroupName = "north america" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);
            result.Should().AllSatisfy(sp => sp.TerritoryId.Should().BeGreaterThan(0));
            totalCount.Should().Be(5);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_filters_by_EmailAddress()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { EmailAddress = "charlie.chen" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Employee.PersonBusinessEntity.EmailAddresses.First().EmailAddressName.Should().Contain("charlie.chen");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_combines_multiple_filters()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel
        {
            FirstName = "bob",
            LastName = "brown",
            SalesTerritoryId = 2
        };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Employee.PersonBusinessEntity.FirstName.Should().Be("Bob");
            result[0].Employee.PersonBusinessEntity.LastName.Should().Be("Brown");
            result[0].TerritoryId.Should().Be(2);
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_returns_empty_when_no_matches()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { FirstName = "NonExistent" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_case_insensitive_search()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel { FirstName = "CHARLIE" };

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Employee.PersonBusinessEntity.FirstName.Should().Be("Charlie");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_honors_paging_parameters()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 2, PageSize = 2 };
        var searchModel = new SalesPersonSearchModel();

        var (result, totalCount) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            totalCount.Should().Be(5);
        }
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_sorts_by_FirstName_ascending()
    {
        var salesPersons = SalesDomainFixtures.GetSalesPersonListForPaging();
        DbContext.SalesPersons.AddRange(salesPersons);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "firstName",
            SortOrder = SortedResultConstants.Ascending
        };
        var searchModel = new SalesPersonSearchModel();

        var (result, _) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        result.Should().BeInAscendingOrder(sp => sp.Employee.PersonBusinessEntity.FirstName);
    }

    [Fact]
    public async Task SearchSalesPersonsAsync_uses_no_tracking()
    {
        var salesPerson = SalesDomainFixtures.GetCompleteSalesPersonEntity();
        DbContext.SalesPersons.Add(salesPerson);
        await DbContext.SaveChangesAsync();

        var parameters = new SalesPersonParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesPersonSearchModel();

        var (result, _) = await _sut.SearchSalesPersonsAsync(parameters, searchModel);

        var entry = DbContext.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion
}
