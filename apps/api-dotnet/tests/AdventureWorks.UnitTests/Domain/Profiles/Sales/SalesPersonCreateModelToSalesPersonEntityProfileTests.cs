using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Models.Slim;

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

    private static SalesPersonCreateModel GetValidModel()
    {
        return new SalesPersonCreateModel
        {
            FirstName = "Jane",
            LastName = "Smith",
            NationalIdNumber = "987654321",
            LoginId = "adventure-works\\jane.smith",
            JobTitle = "Sales Rep",
            BirthDate = new DateTime(1988, 8, 20),
            HireDate = new DateTime(2019, 3, 15),
            MaritalStatus = "M",
            Gender = "F",
            Phone = new SalesPersonPhoneCreateModel
            {
                PhoneNumber = "555-987-6543",
                PhoneNumberTypeId = 1
            },
            EmailAddress = "jane.smith@adventure-works.com",
            Address = new AddressCreateModel
            {
                AddressLine1 = "789 Sales Avenue",
                City = "Seattle",
                PostalCode = "98102",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
            },
            AddressTypeId = 2,
            TerritoryId = 1,
            SalesQuota = 300000,
            Bonus = 5000,
            CommissionPct = 0.02m
        };
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_create_model_to_entity_succeeds_with_all_fields()
    {
        var createModel = GetValidModel();

        var result = _mapper.Map<SalesPersonEntity>(createModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();

            // Verify mapped fields transform correctly
            result.TerritoryId.Should().Be(createModel.TerritoryId, "because TerritoryId should map from model");
            result.SalesQuota.Should().Be(createModel.SalesQuota, "because SalesQuota should map from model");
            result.Bonus.Should().Be(createModel.Bonus, "because Bonus should map from model");
            result.CommissionPct.Should().Be(createModel.CommissionPct, "because CommissionPct should map from model");

            // Verify ignored fields default correctly (no transformation)
            result.BusinessEntityId.Should().Be(0, "because BusinessEntityId should be ignored and default to 0");
            result.SalesYtd.Should().Be(0, "because SalesYtd should be ignored and default to 0");
            result.SalesLastYear.Should().Be(0, "because SalesLastYear should be ignored and default to 0");
            result.Rowguid.Should().Be(Guid.Empty, "because Rowguid should be ignored and default to empty");
            result.ModifiedDate.Should().Be(default, "because ModifiedDate should be ignored");
        }
    }

    [Fact]
    public void Map_create_model_to_entity_succeeds_with_nullable_fields_null()
    {
        var createModel = GetValidModel();
        createModel.TerritoryId = null;
        createModel.SalesQuota = null;

        var result = _mapper.Map<SalesPersonEntity>(createModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BusinessEntityId.Should().Be(0, "because BusinessEntityId should be ignored and default to 0");
            result.TerritoryId.Should().BeNull("because nullable TerritoryId should map as null");
            result.SalesQuota.Should().BeNull("because nullable SalesQuota should map as null");
            result.Bonus.Should().Be(createModel.Bonus);
            result.CommissionPct.Should().Be(createModel.CommissionPct);
        }
    }
}
