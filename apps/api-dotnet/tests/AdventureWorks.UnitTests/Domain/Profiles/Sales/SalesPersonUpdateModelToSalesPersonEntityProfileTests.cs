using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles.Sales;

public sealed class SalesPersonUpdateModelToSalesPersonEntityProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SalesPersonUpdateModelToSalesPersonEntityProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesPersonUpdateModelToSalesPersonEntityProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    private static SalesPersonUpdateModel GetValidUpdateModel(int id = 100)
    {
        return new SalesPersonUpdateModel
        {
            Id = id,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = "A.",
            Title = "Ms.",
            Suffix = "Jr.",
            JobTitle = "Senior Sales Rep",
            MaritalStatus = "M",
            Gender = "F",
            SalariedFlag = true,
            OrganizationLevel = 2,
            TerritoryId = 2,
            SalesQuota = 300000,
            Bonus = 2000,
            CommissionPct = 0.06m
        };
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_update_model_to_existing_entity_preserves_ignored_fields()
    {
        var existingGuid = Guid.NewGuid();
        var existingModifiedDate = DateTime.UtcNow.AddDays(-10);
        const int existingBusinessEntityId = 100;
        const decimal existingSalesYtd = 500000m;
        const decimal existingSalesLastYear = 450000m;

        // Existing entity with data that should NOT be overwritten
        var existingEntity = new SalesPersonEntity
        {
            BusinessEntityId = existingBusinessEntityId,
            TerritoryId = 1,
            SalesQuota = 200000m,
            Bonus = 1000m,
            CommissionPct = 0.03m,
            SalesYtd = existingSalesYtd,
            SalesLastYear = existingSalesLastYear,
            Rowguid = existingGuid,
            ModifiedDate = existingModifiedDate
        };

        // Update model with new values
        var updateModel = GetValidUpdateModel(existingBusinessEntityId);

        // Map update model onto existing entity
        _mapper.Map(updateModel, existingEntity);

        using (new AssertionScope())
        {
            // Verify updated fields changed
            existingEntity.TerritoryId.Should().Be(2, "because TerritoryId should be updated");
            existingEntity.SalesQuota.Should().Be(300000m, "because SalesQuota should be updated");
            existingEntity.Bonus.Should().Be(2000m, "because Bonus should be updated");
            existingEntity.CommissionPct.Should().Be(0.06m, "because CommissionPct should be updated");

            // Verify ignored fields were NOT touched (preserved)
            existingEntity.BusinessEntityId.Should().Be(existingBusinessEntityId, "because BusinessEntityId should be ignored/preserved");
            existingEntity.SalesYtd.Should().Be(existingSalesYtd, "because SalesYtd should be ignored/preserved");
            existingEntity.SalesLastYear.Should().Be(existingSalesLastYear, "because SalesLastYear should be ignored/preserved");
            existingEntity.Rowguid.Should().Be(existingGuid, "because Rowguid should be ignored/preserved");
            existingEntity.ModifiedDate.Should().Be(existingModifiedDate, "because ModifiedDate should be ignored/preserved");
        }
    }

    [Fact]
    public void Map_update_model_with_nulls_clears_nullable_fields()
    {
        var existingEntity = new SalesPersonEntity
        {
            BusinessEntityId = 100,
            TerritoryId = 5,
            SalesQuota = 250000m,
            Bonus = 1000m,
            CommissionPct = 0.05m
        };

        var updateModel = GetValidUpdateModel(100);
        updateModel.TerritoryId = null; // clearing territory
        updateModel.SalesQuota = null; // clearing quota
        updateModel.Bonus = 0;
        updateModel.CommissionPct = 0.04m;

        _mapper.Map(updateModel, existingEntity);

        using (new AssertionScope())
        {
            existingEntity.TerritoryId.Should().BeNull("because nullable field should be cleared when mapped as null");
            existingEntity.SalesQuota.Should().BeNull("because nullable field should be cleared when mapped as null");
            existingEntity.Bonus.Should().Be(0);
            existingEntity.CommissionPct.Should().Be(0.04m);
        }
    }
}
