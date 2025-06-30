using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories.Person;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Person;

[ExcludeFromCodeCoverage]
public sealed class PhoneNumberTypeRepositoryTests : PersistenceUnitTestBase
{
    private readonly PhoneNumberTypeRepository _sut;

    public PhoneNumberTypeRepositoryTests()
    {
        _sut = new PhoneNumberTypeRepository(DbContext);

        DbContext.PhoneNumberTypes.AddRange(new List<PhoneNumberTypeEntity>
        {
            new() { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DateTime.UtcNow }
            ,new() { PhoneNumberTypeId = 2, Name = "Home", ModifiedDate = DateTime.UtcNow }
            ,new() { PhoneNumberTypeId = 3, Name = "Work", ModifiedDate = DateTime.UtcNow }
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(PhoneNumberTypeRepository)
                .Should().Implement<IPhoneNumberTypeRepository>();

            typeof(PhoneNumberTypeRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ListAllAsync_is_correctAsync()
    {
        var result = await _sut.ListAllAsync();

        using (new AssertionScope())
        {
            result.Count.Should().Be(3);
            result.Count(x => x.PhoneNumberTypeId == 3 && x.Name == "Work").Should().Be(1);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetByIdAsync_is_correctAsync(int id)
    {
        var result = await _sut.GetByIdAsync(id);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.PhoneNumberTypeId.Should().Be(id);
        }
    }
}
