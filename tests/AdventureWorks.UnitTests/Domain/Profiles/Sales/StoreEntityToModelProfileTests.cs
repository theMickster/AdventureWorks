using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles.Sales;

[ExcludeFromCodeCoverage]
public sealed class StoreEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public StoreEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var aGuid = Guid.NewGuid();
        const int id = 28;
        const int addressId = 725634;
        const int addressTypeId = 12;

        var entity = new StoreEntity
        {
            BusinessEntityId = id,
            Name = "Colorado Ski, Golf, and Bikes",
            Rowguid = aGuid,
            ModifiedDate = DefaultAuditDate,
            StoreBusinessEntity = new BusinessEntity
            {
                BusinessEntityId = id,
                Rowguid = aGuid,
                ModifiedDate = DefaultAuditDate,
                BusinessEntityAddresses = new List<BusinessEntityAddressEntity>
                {
                    new()
                    {
                        BusinessEntityId = 25,
                        AddressId = addressId,
                        ModifiedDate = DefaultAuditDate,
                        AddressTypeId = addressTypeId,
                        Rowguid = aGuid,
                        AddressType = new AddressTypeEntity
                        {
                            AddressTypeId = addressTypeId, Name = "Storefront Location", ModifiedDate = DefaultAuditDate
                        },
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
                                CountryRegion = new CountryRegionEntity
                                    { CountryRegionCode = "USA", Name = "United States" }
                            },
                            PostalCode = "80232",
                            ModifiedDate = DefaultAuditDate
                        }
                    }
                }
            }
        };

        var result = _mapper.Map<StoreModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Name.Should().Be(entity.Name);
            result.ModifiedDate.Should().Be(entity.ModifiedDate);

            result.StoreAddresses.Count.Should().Be(1);
            result.StoreAddresses[0].Address.Id.Should().Be(addressId);
            result.StoreAddresses[0].AddressType.Id.Should().Be(addressTypeId);

            entity.Rowguid.Should().Be(aGuid);
        }

    }
}
