using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories.Person;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Person;

[ExcludeFromCodeCoverage]
public sealed class ContactTypeRepositoryTests : PersistenceUnitTestBase
{
    private readonly ContactTypeRepository _sut;

    public ContactTypeRepositoryTests()
    {
        _sut = new ContactTypeRepository(DbContext);

        DbContext.ContactTypes.AddRange(new List<ContactTypeEntity>
        {
            new() {ContactTypeId = 1, Name = "One"},
            new() {ContactTypeId = 2, Name = "Two"},
            new() {ContactTypeId = 3, Name = "Three"}
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ContactTypeRepository)
                .Should().Implement<IContactTypeRepository>();

            typeof(ContactTypeRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }
}
