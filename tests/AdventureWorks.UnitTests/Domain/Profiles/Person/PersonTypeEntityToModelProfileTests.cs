using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Models.Person;
using AdventureWorks.Domain.Profiles.Person;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles.Person;

[ExcludeFromCodeCoverage]
public sealed class PersonTypeEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public PersonTypeEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PersonTypeEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();


    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new PersonTypeEntity
        {
            PersonTypeId = 25,
            PersonTypeName = "Individual Customer",
            PersonTypeCode = "IN",
            PersonTypeDescription = "An Individual, retail customer of AdventureWorks Cycling"
        };

        var result = _mapper.Map<PersonTypeModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.PersonTypeId);
            result.Name.Should().Be(entity.PersonTypeName);
            result.Code.Should().Be(entity.PersonTypeCode);
            result.Description.Should().Be(entity.PersonTypeDescription);
        }
    }
}
