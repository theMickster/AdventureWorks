using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class AddressTypeRepositoryTests : PersistenceUnitTestBase
{
    private readonly AddressTypeRepository _sut;

    public AddressTypeRepositoryTests()
    {
        _sut = new AddressTypeRepository(DbContext);

        DbContext.AddressTypes.AddRange(new List<AddressTypeEntity>
            {
                new AddressTypeEntity { AddressTypeId = 1, Name = "Home" },
                new() { AddressTypeId = 2, Name = "Billing" }
            }
        );

        DbContext.SaveChanges();
    }
    
    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(AddressTypeRepository)
                .Should().Implement<IAddressTypeRepository>();

            typeof(AddressTypeRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }
}
