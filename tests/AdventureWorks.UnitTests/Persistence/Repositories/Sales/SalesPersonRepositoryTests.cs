using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

public sealed class SalesPersonRepositoryTests : PersistenceUnitTestBase
{
    private readonly SalesPersonRepository _sut;

    public SalesPersonRepositoryTests()
    {
        _sut = new SalesPersonRepository(DbContext);
        LoadMockSalesPersons();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(StoreRepository)
                .Should().Implement<IStoreRepository>();

            typeof(StoreRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }
}
