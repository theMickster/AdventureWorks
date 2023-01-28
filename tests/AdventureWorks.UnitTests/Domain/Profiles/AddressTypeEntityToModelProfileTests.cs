using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class AddressTypeEntityToModelProfileTests: UnitTestBase
{
    private readonly IMapper _mapper;

    public AddressTypeEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressTypeEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new AddressTypeEntity
        {
            AddressTypeId = 25,
            Name = "Home"
        };

        var result = _mapper.Map<AddressTypeModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(25);
            result.Name.Should().Be("Home");
        }
    }
}
