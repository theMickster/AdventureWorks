using AdventureWorks.Infrastructure.Persistence.Repositories.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Person;

[ExcludeFromCodeCoverage]
public sealed class BusinessEntityContactEntityRepositoryTests : PersistenceUnitTestBase
{
    private readonly BusinessEntityContactEntityRepository _sut;

    public BusinessEntityContactEntityRepositoryTests()
    {
        _sut = new BusinessEntityContactEntityRepository(DbContext);
        LoadMockBusinessEntityContacts();
    }
}
