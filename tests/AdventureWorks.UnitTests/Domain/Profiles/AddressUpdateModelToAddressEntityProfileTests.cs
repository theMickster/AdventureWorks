using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Slim;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class AddressUpdateModelToAddressEntityProfileTests
{
    private readonly IMapper _mapper;

    public AddressUpdateModelToAddressEntityProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressUpdateModelToAddressEntityProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }
        
    [Fact]
    public void All_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();


    [Fact]
    public void Map_model_to_entity_succeeds()
    {
        var model = new AddressUpdateModel
        {
            Id = 797,
            AddressLine1 = "4561 S. Main",
            AddressLine2 = "Apt 2821",
            City = "Denver",
            PostalCode = "82023",
            StateProvince = new GenericSlimModel { Id = 25,Name = string.Empty,Code = string.Empty}
        };

        var result = _mapper.Map<AddressEntity>(model);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.AddressLine1.Should().Be("4561 S. Main");
            result.AddressLine2.Should().Be("Apt 2821");
            result.City.Should().Be("Denver");
            result.StateProvinceId.Should().Be(25);
            result.PostalCode.Should().Be("82023");
            result.AddressId.Should().Be(797);
        }
    }
}