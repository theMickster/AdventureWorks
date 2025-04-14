using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class SalesTerritoryEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SalesTerritoryEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesTerritoryEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var entity = new SalesTerritoryEntity
        {
            TerritoryId = 25,
            Name = "Central",
            CountryRegionCode = "USA",
            CountryRegion = new CountryRegionEntity {CountryRegionCode = "USA", Name = "United States of America"},
            Group = "HelloWorld",
            SalesYtd = 12345.99m,
            SalesLastYear = 54321.97m,
            CostYtd = 65321.59m,
            CostLastYear = 3211.48m
        };

        var result = _mapper.Map<SalesTerritoryModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(25);
            result.Name.Should().Be("Central");
            result.CountryRegion.Code.Should().Be("USA");
            result.CountryRegion.Name.Should().Be("United States of America");
            result.Group.Should().Be("HelloWorld");
            result.SalesYtd.Should().Be(12345.99m);
            result.SalesLastYear.Should().Be(54321.97m);
            result.CostLastYear.Should().Be(3211.48m);

        }
    }

}
