using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class ScrapReasonToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ScrapReasonToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ScrapReasonToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Maps_all_fields_correctly()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new ScrapReason
        {
            ScrapReasonId = (short)5,
            Name = "Color incorrect",
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<ScrapReasonModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ScrapReasonId.Should().Be((short)5);
            result.Name.Should().Be("Color incorrect");
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
