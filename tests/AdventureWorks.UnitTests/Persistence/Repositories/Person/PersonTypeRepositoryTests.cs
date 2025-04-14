using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories.Person;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Person;

[ExcludeFromCodeCoverage]
public sealed class PersonTypeRepositoryTests : PersistenceUnitTestBase
{
    private readonly PersonTypeRepository _sut;

    public PersonTypeRepositoryTests()
    {
        _sut = new PersonTypeRepository(DbContext);

        DbContext.PersonTypes.AddRange( new List<PersonTypeEntity>
        {
            new() {PersonTypeId = 1, PersonTypeName = "One", PersonTypeCode = "O", PersonTypeDescription = "Hello World 1"},
            new() {PersonTypeId = 2, PersonTypeName = "Two", PersonTypeCode = "T", PersonTypeDescription = "Hello World 2"},
            new() {PersonTypeId = 3, PersonTypeName = "Three", PersonTypeCode = "T", PersonTypeDescription = "Hello World 3"}
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(PersonTypeRepository)
                .Should().Implement<IPersonTypeRepository>();

            typeof(PersonTypeRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

}
