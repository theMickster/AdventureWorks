using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;

namespace AdventureWorks.UnitTests.Domain.Profiles;

public sealed class PhoneNumberTypeEntityToPhoneNumberTypeModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public PhoneNumberTypeEntityToPhoneNumberTypeModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PhoneNumberTypeEntityToPhoneNumberTypeModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds()
        => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new PhoneNumberTypeEntity
        {
            PhoneNumberTypeId = 5,
            Name = "Cell",
            ModifiedDate = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        var result = _mapper.Map<PhoneNumberTypeModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(5);
            result.Name.Should().Be("Cell");
            result.ModifiedDate.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0));
        }
    }
}
