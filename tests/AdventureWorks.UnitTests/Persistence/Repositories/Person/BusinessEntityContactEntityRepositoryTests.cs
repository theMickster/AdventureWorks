using AdventureWorks.Infrastructure.Persistence.Repositories.Person;

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

    [Fact]
    public async Task GetContactsByIdAsync_for_single_contact_succeedsAsync()
    {
        const int id = 1112;
        var result = await _sut.GetContactsByIdAsync(id).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Count.Should().Be(1);
            result[0].Person.Should().NotBeNull();
            result[0].Person.PersonType.Should().NotBeNull();
            result[0].ContactType.Should().NotBeNull();

            var owner = result.FirstOrDefault(x => x.PersonId == 12);
            owner.Should().NotBeNull();
            owner!.Person.FirstName.Should().Be("Bill");
            owner!.Person.LastName.Should().Be("Romanowski");

            owner.ContactTypeId.Should().Be(11);
            owner.ContactType.Name.Should().Be("Owner");

            owner!.Person.PersonType.PersonTypeId.Should().Be(4);
            owner!.Person.PersonType.PersonTypeName.Should().Contain("Employee");
        }
    }

    [Fact]
    public async Task GetContactsByIdAsync_for_multiple_contacts_succeedsAsync()
    {
        const int id = 1111;
        var result = await _sut.GetContactsByIdAsync(id).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Count.Should().Be(5);

            result.Count(x => x.ContactTypeId == 19).Should().Be(1);

            var owner = result.FirstOrDefault(x => x.PersonId == 2);
            owner.Should().NotBeNull();
            owner!.Person.FirstName.Should().Be("Terrell");
        }

    }

    [Fact]
    public async Task GetContactsByStoreIdsAsync_succeedsAsync()
    {
        var storeIds = new List<int>{1111, 1112, 1114};

        var results = await _sut.GetContactsByStoreIdsAsync(storeIds).ConfigureAwait(false);

        using (new AssertionScope())
        {
            results.Should().HaveCount(8);

            results.Count(x => x.BusinessEntityId == 1113).Should().Be(0);
        }

    }
}

