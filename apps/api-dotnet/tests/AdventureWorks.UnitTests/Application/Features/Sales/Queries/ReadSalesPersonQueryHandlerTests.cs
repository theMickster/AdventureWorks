using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesPersonQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private ReadSalesPersonQueryHandler _sut;

    public ReadSalesPersonQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesPersonEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadSalesPersonQueryHandler(_mapper, _mockSalesPersonRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesPersonQueryHandler(null!, _mockSalesPersonRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesPersonQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesPersonRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_when_entity_not_found_Async()
    {
        const int salesPersonId = 999;

        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonByIdAsync(salesPersonId))
            .ReturnsAsync((SalesPersonEntity)null!);

        var result = await _sut.Handle(new ReadSalesPersonQuery { Id = salesPersonId }, CancellationToken.None);

        result?.Should().BeNull("because the entity was not found in the repository");
    }

    [Fact]
    public async Task Handle_returns_valid_model_with_complete_data_Async()
    {
        const int businessEntityId = 100;
        const string title = "Ms.";
        const string firstName = "Jane";
        const string middleName = "A.";
        const string lastName = "Doe";
        const string suffix = "Jr.";
        const string jobTitle = "Sales Representative";
        const string email = "jane.doe@adventure-works.com";
        const int territoryId = 5;
        const decimal salesQuota = 250000m;
        const decimal bonus = 5000m;
        const decimal commissionPct = 0.05m;
        var modifiedDate = DefaultAuditDate;

        // Build complete entity graph
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
                    Rowguid = Guid.NewGuid(),
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
            TerritoryId = territoryId,
            SalesQuota = salesQuota,
            Bonus = bonus,
            CommissionPct = commissionPct,
            Employee = employeeEntity,
            ModifiedDate = modifiedDate
        };

        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonByIdAsync(businessEntityId))
            .ReturnsAsync(salesPersonEntity);

        var result = await _sut.Handle(new ReadSalesPersonQuery { Id = businessEntityId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the entity was found");
            result!.Id.Should().Be(businessEntityId, "because Id should map from BusinessEntityId");

            // Verify person data transformation
            result.Title.Should().Be(title, "because Title should transform from Employee.Person.Title");
            result.FirstName.Should().Be(firstName, "because FirstName should transform correctly");
            result.MiddleName.Should().Be(middleName, "because MiddleName should transform correctly");
            result.LastName.Should().Be(lastName, "because LastName should transform correctly");
            result.Suffix.Should().Be(suffix, "because Suffix should transform correctly");
            result.JobTitle.Should().Be(jobTitle, "because JobTitle should transform from Employee");
            result.EmailAddress.Should().Be(email, "because EmailAddress should resolve from nested collection");

            result.ModifiedDate.Should().Be(modifiedDate, "because ModifiedDate should transform correctly");
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task Handle_calls_repository_with_correct_id(int salesPersonId)
    {
        var entity = new SalesPersonEntity
        {
            BusinessEntityId = salesPersonId,
            Employee = new EmployeeEntity
            {
                PersonBusinessEntity = new PersonEntity { FirstName = "Test", LastName = "User" }
            }
        };

        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonByIdAsync(salesPersonId))
            .ReturnsAsync(entity);

        await _sut.Handle(new ReadSalesPersonQuery { Id = salesPersonId }, CancellationToken.None);

        _mockSalesPersonRepository.Verify(
            x => x.GetSalesPersonByIdAsync(salesPersonId),
            Times.Once,
            "because the handler should call repository with the exact ID from the query");
    }
}
