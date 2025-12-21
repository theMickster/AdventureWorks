using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class CountryRegionEntityToCountryRegionModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public CountryRegionEntityToCountryRegionModelProfileTests()
    {
        _mapper = SharedMapper;
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new CountryRegionEntity
        {
            CountryRegionCode = "USA",
            Name = "United States of America",
            ModifiedDate = DateTime.UtcNow,
            StateProvinces = new List<StateProvinceEntity>()
        };

        var result = _mapper.Map<CountryRegionModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Code.Should().Be("USA");
            result.Name.Should().Be("United States of America");
        }
    }
}