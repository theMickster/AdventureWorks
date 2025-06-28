using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles.Sales;

public sealed class SalesPersonCreateModelToSalesPersonEntityProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SalesPersonCreateModelToSalesPersonEntityProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesPersonCreateModelToSalesPersonEntityProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_create_model_to_entity_succeeds_with_all_fields()
    {
        const int businessEntityId = 100;
        const int territoryId = 5;
        const decimal salesQuota = 250000m;
        const decimal bonus = 5000m;
        const decimal commissionPct = 0.05m;

        var createModel = new SalesPersonCreateModel
        {
            BusinessEntityId = businessEntityId,
            TerritoryId = territoryId,
            SalesQuota = salesQuota,
            Bonus = bonus,
            CommissionPct = commissionPct
        };

        var result = _mapper.Map<SalesPersonEntity>(createModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();

            // Verify mapped fields transform correctly
            result.BusinessEntityId.Should().Be(businessEntityId, "because BusinessEntityId should map from model");
            result.TerritoryId.Should().Be(territoryId, "because TerritoryId should map from model");
            result.SalesQuota.Should().Be(salesQuota, "because SalesQuota should map from model");
            result.Bonus.Should().Be(bonus, "because Bonus should map from model");
            result.CommissionPct.Should().Be(commissionPct, "because CommissionPct should map from model");

            // Verify ignored fields default correctly (no transformation)
            result.SalesYtd.Should().Be(0, "because SalesYtd should be ignored and default to 0");
            result.SalesLastYear.Should().Be(0, "because SalesLastYear should be ignored and default to 0");
            result.Rowguid.Should().Be(Guid.Empty, "because Rowguid should be ignored and default to empty");
            result.ModifiedDate.Should().Be(default, "because ModifiedDate should be ignored");
        }
    }

    [Fact]
    public void Map_create_model_to_entity_succeeds_with_nullable_fields_null()
    {
        const int businessEntityId = 200;

        var createModel = new SalesPersonCreateModel
        {
            BusinessEntityId = businessEntityId,
            TerritoryId = null, // nullable
            SalesQuota = null, // nullable
            Bonus = 0,
            CommissionPct = 0.03m
        };

        var result = _mapper.Map<SalesPersonEntity>(createModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BusinessEntityId.Should().Be(businessEntityId);
            result.TerritoryId.Should().BeNull("because nullable TerritoryId should map as null");
            result.SalesQuota.Should().BeNull("because nullable SalesQuota should map as null");
            result.Bonus.Should().Be(0);
            result.CommissionPct.Should().Be(0.03m);
        }
    }
}
