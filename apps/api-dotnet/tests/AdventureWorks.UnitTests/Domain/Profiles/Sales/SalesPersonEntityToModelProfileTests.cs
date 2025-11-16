using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Domain.Profiles.Sales;
public sealed class SalesPersonEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public SalesPersonEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesPersonEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        var aGuid = Guid.NewGuid();
        const int businessEntityId = 100;
        const string title = "Ms.";
        const string firstName = "Jane";
        const string middleName = "A.";
        const string lastName = "Doe";
        const string suffix = "Jr.";
        const string jobTitle = "Sales Manager";
        const string email = "jane.doe@adventure-works.com";
        var modifiedDate = DefaultAuditDate;

        var personEntity = new PersonEntity
        {
            BusinessEntityId = businessEntityId,
            Title = title,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Suffix = suffix,
            EmailAddresses = new List<EmailAddressEntity>
            {
                new()
                {
                    BusinessEntityId = businessEntityId,
                    EmailAddressId = 1,
                    EmailAddressName = email,
                    Rowguid = aGuid,
                    ModifiedDate = modifiedDate
                }
            },
            ModifiedDate = modifiedDate
        };

        var employeeEntity = new EmployeeEntity
        {
            BusinessEntityId = businessEntityId,
            JobTitle = jobTitle,
            PersonBusinessEntity = personEntity,
            ModifiedDate = modifiedDate
        };

        var salesPersonEntity = new SalesPersonEntity
        {
            BusinessEntityId = businessEntityId,
            Employee = employeeEntity,
            SalesYtd = 50000m,
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<SalesPersonModel>(salesPersonEntity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(businessEntityId);
            result.Title.Should().Be(title);
            result.FirstName.Should().Be(firstName);
            result.MiddleName.Should().Be(middleName);
            result.LastName.Should().Be(lastName);
            result.Suffix.Should().Be(suffix);
            result.JobTitle.Should().Be(jobTitle);
            result.EmailAddress.Should().Be(email);
            result.ModifiedDate.Should().Be(modifiedDate);
            result.SalesYtd.Should().Be(50000m);
            result.TerritoryName.Should().BeNull();
            result.Bonus.Should().Be(0m);
            result.CommissionPct.Should().Be(0m);
        }
    }

    [Fact]
    public void Map_entity_with_territory_to_model_maps_territory_name_succeeds()
    {
        var aGuid = Guid.NewGuid();
        const int businessEntityId = 101;
        var modifiedDate = DefaultAuditDate;

        var personEntity = new PersonEntity
        {
            BusinessEntityId = businessEntityId,
            FirstName = "John",
            LastName = "Smith",
            EmailAddresses = new List<EmailAddressEntity>
            {
                new()
                {
                    BusinessEntityId = businessEntityId,
                    EmailAddressId = 2,
                    EmailAddressName = "john.smith@adventure-works.com",
                    Rowguid = aGuid,
                    ModifiedDate = modifiedDate
                }
            },
            ModifiedDate = modifiedDate
        };

        var employeeEntity = new EmployeeEntity
        {
            BusinessEntityId = businessEntityId,
            JobTitle = "Sales Representative",
            PersonBusinessEntity = personEntity,
            ModifiedDate = modifiedDate
        };

        var salesPersonEntity = new SalesPersonEntity
        {
            BusinessEntityId = businessEntityId,
            Employee = employeeEntity,
            SalesTerritory = new SalesTerritoryEntity
            {
                TerritoryId = 1,
                Name = "Northeast",
                CountryRegionCode = "US",
                Group = "North America",
                ModifiedDate = modifiedDate
            },
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<SalesPersonModel>(salesPersonEntity);

        result.TerritoryName.Should().Be("Northeast");
    }
}
