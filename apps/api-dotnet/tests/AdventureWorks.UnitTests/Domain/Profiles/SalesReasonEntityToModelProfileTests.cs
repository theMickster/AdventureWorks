using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class SalesReasonEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SalesReasonEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesReasonEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new SalesReason
        {
            SalesReasonId = 1,
            Name = "Price",
            ReasonType = "Other",
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<SalesReasonModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.SalesReasonId.Should().Be(1);
            result.Name.Should().Be("Price");
            result.ReasonType.Should().Be("Other");
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
