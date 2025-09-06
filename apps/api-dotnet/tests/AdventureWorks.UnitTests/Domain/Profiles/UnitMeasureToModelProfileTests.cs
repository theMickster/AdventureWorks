using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class UnitMeasureToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public UnitMeasureToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(UnitMeasureToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Maps_all_fields_correctly()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new UnitMeasure
        {
            UnitMeasureCode = "EA",
            Name = "Each",
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<UnitMeasureModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.UnitMeasureCode.Should().Be("EA");
            result.Name.Should().Be("Each");
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
