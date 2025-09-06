using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class LocationToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public LocationToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(LocationToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Maps_all_fields_correctly()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new Location
        {
            LocationId = (short)7,
            Name = "Subassembly",
            CostRate = 12.25m,
            Availability = 400.00m,
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<LocationModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.LocationId.Should().Be((short)7);
            result.Name.Should().Be("Subassembly");
            result.CostRate.Should().Be(12.25m);
            result.Availability.Should().Be(400.00m);
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
