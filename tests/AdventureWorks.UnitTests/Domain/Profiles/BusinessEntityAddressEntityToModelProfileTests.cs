using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class BusinessEntityAddressEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public BusinessEntityAddressEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BusinessEntityAddressEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var aGuid = Guid.NewGuid();
        const int addressId = 725634;
        const int addressTypeId = 12;
        var entity = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 25,
            AddressId = addressId,
            ModifiedDate = DefaultAuditDate,
            AddressTypeId = addressTypeId,
            Rowguid = aGuid,
            AddressType = new AddressTypeEntity{AddressTypeId = addressTypeId, Name = "Storefront Location",ModifiedDate = DefaultAuditDate},
            Address = new AddressEntity
            {
                AddressId = addressId,
                AddressLine1 = "1234 Hello World",
                AddressLine2 = "Apt 1",
                City = "Denver",
                StateProvinceId = 26,
                StateProvince = new StateProvinceEntity
                {
                    StateProvinceId = 26,
                    StateProvinceCode = "CO",
                    CountryRegionCode = "USA",
                    CountryRegion = new CountryRegionEntity { CountryRegionCode = "USA", Name = "United States" }
                },
                PostalCode = "80232",
                ModifiedDate = DefaultAuditDate
            }
        };

        var result = _mapper.Map<BusinessEntityAddressModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Address.Id.Should().Be(addressId);
            result.Address.City.Should().Be("Denver");
            result.Address.CountryRegion.Should().NotBeNull();
            result.Address.AddressStateProvince.Should().NotBeNull();

            result.AddressType.Id.Should().Be(addressTypeId);
            result.AddressType.Name.Should().Be("Storefront Location");

            entity.Rowguid.Should().Be(aGuid);
        }
    }
}
