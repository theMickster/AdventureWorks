using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles.Person;

[ExcludeFromCodeCoverage]
public sealed class ContactTypeEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ContactTypeEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ContactTypeEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new ContactTypeEntity
        {
            ContactTypeId = 7,
            Name = "Hello World"
        };

        var result = _mapper.Map<ContactTypeModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.ContactTypeId);
            result.Name.Should().Be(entity.Name);
        }
    }
}
