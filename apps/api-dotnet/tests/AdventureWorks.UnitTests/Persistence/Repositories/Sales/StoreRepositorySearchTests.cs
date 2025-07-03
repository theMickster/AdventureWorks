using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

[ExcludeFromCodeCoverage]
public sealed class StoreRepositorySearchTests : PersistenceUnitTestBase
{
    private readonly StoreRepository _sut;

    public StoreRepositorySearchTests()
    {
        _sut = new StoreRepository(DbContext);
        
        DbContext.AddressTypes.AddRange(LookupFixtures.GetAddressTypeEntities());
        DbContext.ContactTypes.AddRange(LookupFixtures.GetContactTypeEntities());
        DbContext.PersonTypes.AddRange(LookupFixtures.GetPersonTypeEntities());
        DbContext.CountryRegions.AddRange(LookupFixtures.GetCountryRegionsEntities());
        DbContext.StateProvinces.AddRange(LookupFixtures.GetStateProvinceEntities());

        DbContext.Stores.AddRange(SalesDomainFixtures.GetStores());
        DbContext.BusinessEntities.AddRange(SalesDomainFixtures.GetStoreBusinessEntities());
        DbContext.BusinessEntityContacts.AddRange(SalesDomainFixtures.GetBusinessEntityContactEntities());
        DbContext.BusinessEntityAddresses.AddRange(SalesDomainFixtures.GetBusinessEntityAddressEntities());
        DbContext.Addresses.AddRange(SalesDomainFixtures.GetAddressEntities());
        DbContext.SalesPersons.AddRange(SalesDomainFixtures.GetSalesPersonEntities());

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task GetStoresAsync_succeeds_when_sorted_by_BusinessEntityIdAsync()
    {
        var ascParams = new StoreParameter { OrderBy = "id", SortOrder = "asc", PageSize = 25, PageNumber = 1 };
        var descParams = new StoreParameter { OrderBy = "id", SortOrder = "descending", PageSize = 25, PageNumber = 1 };

        var (ascResults, ascTotalCount) = await _sut.GetStoresAsync(ascParams);
        var (descResults, descTotalCount) = await _sut.GetStoresAsync(descParams);

        var firstAscRecord = ascResults.First();
        var lastAscRecord = ascResults.Last();

        var firstDescRecord = descResults.First();
        var lastDescRecord = descResults.Last();

        using (new AssertionScope())
        {
            ascParams.SortOrder.Should().Be(SortedResultConstants.Ascending);
            descParams.SortOrder.Should().Be(SortedResultConstants.Descending);

            ascTotalCount.Should().BeGreaterThan(350);
            descTotalCount.Should().BeGreaterThan(350);

            ascResults.Should().HaveCount(25);
            descResults.Should().HaveCount(25);

            firstAscRecord!.Should().NotBeNull();
            lastAscRecord!.Should().NotBeNull();

            firstAscRecord.BusinessEntityId.Should().Be(292);
            lastAscRecord.BusinessEntityId.Should().Be(340);

            firstDescRecord!.Should().NotBeNull();
            lastDescRecord!.Should().NotBeNull();

            firstDescRecord.BusinessEntityId.Should().Be(998);
            lastDescRecord.BusinessEntityId.Should().Be(950);

        }
    }

    [Fact]
    public async Task GetStoresAsync_succeeds_when_sorted_by_NameAsync()
    {
        var ascParams = new StoreParameter { OrderBy = "name", SortOrder = "asc", PageSize = 25, PageNumber = 1 };
        var descParams = new StoreParameter { OrderBy = "name", SortOrder = "descending", PageSize = 25, PageNumber = 1 };

        var (ascResults, ascTotalCount) = await _sut.GetStoresAsync(ascParams);
        var (descResults, descTotalCount) = await _sut.GetStoresAsync(descParams);

        var firstAscRecord = ascResults.First();
        var lastAscRecord = ascResults.Last();

        var firstDescRecord = descResults.First();
        var lastDescRecord = descResults.Last();

        using (new AssertionScope())
        {
            ascParams.SortOrder.Should().Be(SortedResultConstants.Ascending);
            descParams.SortOrder.Should().Be(SortedResultConstants.Descending);

            ascTotalCount.Should().BeGreaterThan(350);
            descTotalCount.Should().BeGreaterThan(350);

            ascResults.Should().HaveCount(25);
            descResults.Should().HaveCount(25);

            firstAscRecord!.Should().NotBeNull();
            lastAscRecord!.Should().NotBeNull();

            firstAscRecord.Name.Should().Be("A Bike Store");
            lastAscRecord.Name.Should().Be("Bike Goods");

            firstDescRecord!.Should().NotBeNull();
            lastDescRecord!.Should().NotBeNull();

            firstDescRecord.Name.Should().Be("Year-Round Sports");
            lastDescRecord.Name.Should().Be("Utilitarian Sporting Goods");
        }
    }

    [Theory]
    [InlineData(330, "ascending")]
    [InlineData(428, "desc")]
    public async Task SearchStoresAsync_by_id_succeedsAsync(int storeId, string sortOrder)
    {
        var queryParams = new StoreParameter { OrderBy = "id", SortOrder = sortOrder };
        var searchParams = new StoreSearchModel { Id = storeId };

        var (ascResults, totalCount) = await _sut.SearchStoresAsync(queryParams, searchParams);
        var firstRecord = ascResults.First();

        using (new AssertionScope())
        {
            totalCount.Should().Be(1);
            firstRecord!.Should().NotBeNull();
            firstRecord!.BusinessEntityId.Should().Be(storeId);
        }
    }

    [Theory]
    [InlineData("bike SHOP", "ascending", 904, 13)]
    [InlineData("exercisE", "desc", 828, 3)]
    public async Task SearchStoresAsync_by_name_succeedsAsync(string storeName, string sortOrder, int firstStoreId, int totalCount)
    {
        var queryParams = new StoreParameter { OrderBy = "Name", SortOrder = sortOrder };
        var searchParams = new StoreSearchModel { Name = storeName };

        var (ascResults, totalCountOutput) = await _sut.SearchStoresAsync(queryParams, searchParams);
        var firstRecord = ascResults.First();

        using (new AssertionScope())
        {
            totalCountOutput.Should().Be(totalCount);
            firstRecord!.Should().NotBeNull();
            firstRecord!.BusinessEntityId.Should().Be(firstStoreId);
        }
    }
}
