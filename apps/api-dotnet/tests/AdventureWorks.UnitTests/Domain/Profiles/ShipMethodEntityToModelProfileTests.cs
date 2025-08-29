using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class ShipMethodEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ShipMethodEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ShipMethodEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new ShipMethod
        {
            ShipMethodId = 1,
            Name = "CARGO TRANSPORT 5",
            ShipBase = 3.95m,
            ShipRate = 1.25m,
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<ShipMethodModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ShipMethodId.Should().Be(1);
            result.Name.Should().Be("CARGO TRANSPORT 5");
            result.ShipBase.Should().Be(3.95m);
            result.ShipRate.Should().Be(1.25m);
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
