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
            typeof(SalesPersonRepository)
                .Should().Implement<ISalesPersonRepository>();

            typeof(SalesPersonRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }
}
