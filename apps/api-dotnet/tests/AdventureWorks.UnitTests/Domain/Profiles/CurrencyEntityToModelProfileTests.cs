using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class CurrencyEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public CurrencyEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(CurrencyEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new Currency
        {
            CurrencyCode = "USD",
            Name = "US Dollar",
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<CurrencyModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.CurrencyCode.Should().Be("USD");
            result.Name.Should().Be("US Dollar");
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
