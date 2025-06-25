using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class StateProvinceEntityToStateProvinceModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public StateProvinceEntityToStateProvinceModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StateProvinceEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new StateProvinceEntity
        {
            StateProvinceId = 10,
            StateProvinceCode = "CO",
            Name = "Colorado",
            CountryRegionCode = "US",
            IsOnlyStateProvinceFlag = false,
            TerritoryId = 3,
            SalesTerritory = new SalesTerritoryEntity {TerritoryId = 3, Name = "Central", CountryRegionCode = "US"},
            CountryRegion = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States" },
            ModifiedDate = DateTime.UtcNow
        };

        var result = _mapper.Map<StateProvinceModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(10);
            result.Name.Should().Be("Colorado");
            result.IsStateProvinceCodeUnavailable.Should().BeFalse();
            result.CountryRegion.Code.Should().Be("US");
            result.CountryRegion.Name.Should().Be("United States");
            result.Territory.Id.Should().Be(3);
            result.Territory.Name.Should().Be("Central");
            result.Territory.Code?.Should().BeEmpty();
        }
    }
}
