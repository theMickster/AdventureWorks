using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private ReadEmployeeQueryHandler _sut;

    public ReadEmployeeQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeeQueryHandler(_mapper, _mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadEmployeeQueryHandler(null!, _mockEmployeeRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadEmployeeQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("employeeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_when_entity_not_found_Async()
    {
        const int businessEntityId = 999;

        _mockEmployeeRepository.Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity)null!);

        var result = await _sut.Handle(
            new ReadEmployeeQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        result?.Should().BeNull("because the entity was not found in the repository");
    }

    [Fact]
    public async Task Handle_throws_exception_when_request_is_null_Async()
    {
        await ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>("because request cannot be null");
    }

    [Fact]
    public async Task Handle_returns_valid_model_with_complete_data_Async()
    {
        const int businessEntityId = 100;
        const string title = "Ms.";
        const string firstName = "Jane";
        const string middleName = "Marie";
        const string lastName = "Doe";
        const string suffix = "Jr.";
        const string nationalIdNumber = "987654321";
        const string loginId = "adventure-works\\jane.doe";
        const string jobTitle = "Sales Representative";
        const string email = "jane.doe@adventure-works.com";
        const string maritalStatus = "S";
        const string gender = "F";
        const bool salariedFlag = true;
        const bool currentFlag = true;
        const short vacationHours = 40;
        const short sickLeaveHours = 20;
        short? organizationLevel = 3;
        var birthDate = new DateTime(1988, 3, 20);
        var hireDate = new DateTime(2019, 5, 15);
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
            NationalIdnumber = nationalIdNumber,
            LoginId = loginId,
            JobTitle = jobTitle,
            BirthDate = birthDate,
            HireDate = hireDate,
            MaritalStatus = maritalStatus,
            Gender = gender,
            SalariedFlag = salariedFlag,
            OrganizationLevel = organizationLevel,
            CurrentFlag = currentFlag,
            VacationHours = vacationHours,
            SickLeaveHours = sickLeaveHours,
            PersonBusinessEntity = personEntity,
            ModifiedDate = modifiedDate
        };

        _mockEmployeeRepository.Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        var result = await _sut.Handle(
            new ReadEmployeeQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the entity was found");
            result!.Id.Should().Be(businessEntityId, "because Id should map from BusinessEntityId");

            // Verify person data transformation
            result.Title.Should().Be(title, "because Title should transform from PersonBusinessEntity.Title");
            result.FirstName.Should().Be(firstName, "because FirstName should transform correctly");
            result.MiddleName.Should().Be(middleName, "because MiddleName should transform correctly");
            result.LastName.Should().Be(lastName, "because LastName should transform correctly");
            result.Suffix.Should().Be(suffix, "because Suffix should transform correctly");

            // Verify employee data transformation
            result.NationalIdNumber.Should().Be(nationalIdNumber, "because NationalIdNumber should transform correctly");
            result.LoginId.Should().Be(loginId, "because LoginId should transform correctly");
            result.JobTitle.Should().Be(jobTitle, "because JobTitle should transform correctly");
            result.BirthDate.Should().Be(birthDate, "because BirthDate should transform correctly");
            result.HireDate.Should().Be(hireDate, "because HireDate should transform correctly");
            result.MaritalStatus.Should().Be(maritalStatus, "because MaritalStatus should transform correctly");
            result.Gender.Should().Be(gender, "because Gender should transform correctly");
            result.SalariedFlag.Should().Be(salariedFlag, "because SalariedFlag should transform correctly");
            result.OrganizationLevel.Should().Be(organizationLevel, "because OrganizationLevel should transform correctly");
            result.CurrentFlag.Should().Be(currentFlag, "because CurrentFlag should transform correctly");
            result.VacationHours.Should().Be(vacationHours, "because VacationHours should transform correctly");
            result.SickLeaveHours.Should().Be(sickLeaveHours, "because SickLeaveHours should transform correctly");

            result.EmailAddress.Should().Be(email, "because EmailAddress should resolve from nested collection");
            result.ModifiedDate.Should().Be(modifiedDate, "because ModifiedDate should transform correctly");
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task Handle_calls_repository_with_correct_id(int businessEntityId)
    {
        var entity = new EmployeeEntity
        {
            BusinessEntityId = businessEntityId,
            NationalIdnumber = "123456789",
            LoginId = "test\\user",
            JobTitle = "Test Job",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            HireDate = DateTime.UtcNow.AddYears(-5),
            MaritalStatus = "S",
            Gender = "M",
            PersonBusinessEntity = new PersonEntity
            {
                FirstName = "Test",
                LastName = "User"
            }
        };

        _mockEmployeeRepository.Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.Handle(
            new ReadEmployeeQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        _mockEmployeeRepository.Verify(
            x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()),
            Times.Once,
            "because the handler should call repository with the exact BusinessEntityId from the query");
    }

    [Fact]
    public async Task Handle_maps_employee_with_null_optional_fields_Async()
    {
        const int businessEntityId = 200;

        var personEntity = new PersonEntity
        {
            BusinessEntityId = businessEntityId,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = null, // Optional field
            Title = null, // Optional field
            Suffix = null, // Optional field
            EmailAddresses = null // Optional collection
        };

        var employeeEntity = new EmployeeEntity
        {
            BusinessEntityId = businessEntityId,
            NationalIdnumber = "111222333",
            LoginId = "test\\jane.smith",
            JobTitle = "Analyst",
            BirthDate = new DateTime(1992, 8, 10),
            HireDate = new DateTime(2021, 3, 1),
            MaritalStatus = "M",
            Gender = "F",
            OrganizationLevel = null, // Optional field
            PersonBusinessEntity = personEntity
        };

        _mockEmployeeRepository.Setup(x => x.GetEmployeeByIdAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        var result = await _sut.Handle(
            new ReadEmployeeQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.MiddleName.Should().BeNull("because MiddleName is optional");
            result.Title.Should().BeNull("because Title is optional");
            result.Suffix.Should().BeNull("because Suffix is optional");
            result.OrganizationLevel.Should().BeNull("because OrganizationLevel is optional");
            result.EmailAddress.Should().BeNull("because EmailAddresses collection is null");
        }
    }
}
