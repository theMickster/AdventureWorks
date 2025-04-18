using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class AddressEntityToAddressModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public AddressEntityToAddressModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressEntityToAddressModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void map_entities_to_model_succeeds()
    {
        const int id = 9865;
        var modified = new DateTime(2022, 11, 11, 11, 11, 11);

        var entity = new AddressEntity
        {
            AddressId = id,
            AddressLine1 = "1234 Hello World",
            AddressLine2 = "Apt 1",
            City = "Denver",
            StateProvinceId = 26,
            StateProvince = new StateProvinceEntity
            {
                StateProvinceId = 26, 
                StateProvinceCode = "CO",
                CountryRegionCode = "USA",
                CountryRegion = new CountryRegionEntity {CountryRegionCode = "USA", Name = "United States"}
            },
            PostalCode = "80232",
            ModifiedDate = modified
        };

        var result = _mapper.Map<AddressModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.AddressLine1.Should().Be("1234 Hello World");
            result.AddressLine2.Should().Be("Apt 1");
            result.City.Should().Be("Denver");
            result.AddressStateProvince.Id.Should().Be(26);
            result.AddressStateProvince.Code.Should().Be("CO");
            result.CountryRegion.Code.Should().Be("USA");
            result.CountryRegion.Name.Should().Be("United States");
            result.PostalCode.Should().Be("80232");
            result.ModifiedDate.Should().Be(modified);
        }
    }
}
