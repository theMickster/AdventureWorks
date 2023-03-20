using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Models.Sales;
using AdventureWorks.Domain.Profiles.Sales;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles.Sales;

[ExcludeFromCodeCoverage]
public sealed class BusinessEntityContactEntityToStoreContactModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public BusinessEntityContactEntityToStoreContactModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BusinessEntityContactEntityToStoreContactModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var aGuid = Guid.NewGuid();
        const int id = 28;
        const int personId = 725634;
        const int contactTypeId = 12;

        var entity = new BusinessEntityContactEntity
        {
            BusinessEntityId = id,
            PersonId = personId,
            ContactTypeId = contactTypeId,
            Rowguid = aGuid,
            ModifiedDate = DefaultAuditDate,
            Person = new PersonEntity
            {
                BusinessEntityId = personId,
                Title = "Mr.",
                FirstName = "Mickey",
                MiddleName = "Charles",
                LastName = "Mantle",
                Suffix = "..",
                Rowguid = aGuid,
                ModifiedDate = DefaultAuditDate
            },
            ContactType = new ContactTypeEntity()
            {
                ContactTypeId = contactTypeId,
                Name = "Owner"
            }
        };

        var result = _mapper.Map<StoreContactModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.ContactTypeId.Should().Be(entity.ContactTypeId);
            result.ContactTypeName.Should().Be("Owner");
            result.Title.Should().Be(entity.Person.Title);
            result.FirstName.Should().Be(entity.Person.FirstName);
            result.MiddleName.Should().Be(entity.Person.MiddleName);
            result.LastName.Should().Be(entity.Person.LastName);
            result.Suffix.Should().Be(entity.Person.Suffix);

            entity.Rowguid.Should().Be(aGuid);
            entity.BusinessEntityId.Should().Be(id);
            entity.PersonId.Should().Be(personId);
            entity.ContactTypeId.Should().Be(contactTypeId);
        }
    }

    [Fact]
    public void Map_entities_to_model_succeeds_when_relate_entities_are_null()
    {
        var aGuid = Guid.NewGuid();
        const int id = 28;
        const int personId = 725634;
        const int contactTypeId = 12;

        var entity = new BusinessEntityContactEntity
        {
            BusinessEntityId = id,
            PersonId = personId,
            ContactTypeId = contactTypeId,
            Rowguid = aGuid,
            ModifiedDate = DefaultAuditDate
        };

        var result = _mapper.Map<StoreContactModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.ContactTypeId.Should().Be(entity.ContactTypeId);
            result.ContactTypeName.Should().BeNull();
            result.Title.Should().BeNull();
            result.FirstName.Should().BeNull();
            result.MiddleName.Should().BeNull();
            result.LastName.Should().BeNull();
            result.Suffix.Should().BeNull();

            entity.Rowguid.Should().Be(aGuid);
            entity.BusinessEntityId.Should().Be(id);
            entity.PersonId.Should().Be(personId);
            entity.ContactTypeId.Should().Be(contactTypeId);
        }
    }

}
