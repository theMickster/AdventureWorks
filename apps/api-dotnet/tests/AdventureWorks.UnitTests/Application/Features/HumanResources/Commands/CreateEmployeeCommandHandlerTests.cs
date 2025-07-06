using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Constants;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class CreateEmployeeCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeeCreateModel>> _mockValidator = new();
    private CreateEmployeeCommandHandler _sut;

    public CreateEmployeeCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<EmployeeCreateModelToEmployeeEntityProfile>();
            c.AddProfile<AddressCreateModelToAddressEntityProfile>();
        });
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateEmployeeCommandHandler(_mapper, _mockEmployeeRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_exception_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_exception_when_request_model_is_nullAsync()
    {
        var command = new CreateEmployeeCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_is_invalidAsync()
    {
        var command = new CreateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel(),
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        var validator = new FakeFailureValidator<EmployeeCreateModel>(
            "NationalIdNumber",
            "National ID number cannot be greater than 15 characters");

        _sut = new CreateEmployeeCommandHandler(_mapper, _mockEmployeeRepository.Object, validator);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "National ID number cannot be greater than 15 characters")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_business_entity_id_when_successfulAsync()
    {
        var command = new CreateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel(),
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        const int expectedBusinessEntityId = 100;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBusinessEntityId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(expectedBusinessEntityId);
    }

    [Fact]
    public async Task Handle_maps_employee_model_to_entity_correctlyAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        var command = new CreateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        EmployeeEntity? capturedEmployeeEntity = null;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeEntity, PersonEntity, PersonPhone, EmailAddressEntity, AddressEntity, int, DateTime, Guid, CancellationToken>(
                (emp, _, _, _, _, _, _, _, _) => capturedEmployeeEntity = emp)
            .ReturnsAsync(100);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEmployeeEntity.Should().NotBeNull();
            capturedEmployeeEntity!.NationalIdnumber.Should().Be(model.NationalIdNumber);
            capturedEmployeeEntity.LoginId.Should().Be(model.LoginId);
            capturedEmployeeEntity.JobTitle.Should().Be(model.JobTitle);
            capturedEmployeeEntity.BirthDate.Should().Be(model.BirthDate);
            capturedEmployeeEntity.HireDate.Should().Be(HumanResourcesConstants.TemporaryHireDate);
            capturedEmployeeEntity.MaritalStatus.Should().Be(model.MaritalStatus);
            capturedEmployeeEntity.Gender.Should().Be(model.Gender);
            capturedEmployeeEntity.OrganizationLevel.Should().Be(model.OrganizationLevel);
        }
    }

    [Fact]
    public async Task Handle_creates_person_entity_correctlyAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        var command = new CreateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        PersonEntity? capturedPersonEntity = null;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeEntity, PersonEntity, PersonPhone, EmailAddressEntity, AddressEntity, int, DateTime, Guid, CancellationToken>(
                (_, person, _, _, _, _, _, _, _) => capturedPersonEntity = person)
            .ReturnsAsync(100);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedPersonEntity.Should().NotBeNull();
            capturedPersonEntity!.FirstName.Should().Be(model.FirstName);
            capturedPersonEntity.LastName.Should().Be(model.LastName);
            capturedPersonEntity.MiddleName.Should().Be(model.MiddleName);
            capturedPersonEntity.Title.Should().Be(model.Title);
            capturedPersonEntity.Suffix.Should().Be(model.Suffix);
        }
    }

    [Fact]
    public async Task Handle_creates_person_phone_correctlyAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        var command = new CreateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        PersonPhone? capturedPhone = null;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeEntity, PersonEntity, PersonPhone, EmailAddressEntity, AddressEntity, int, DateTime, Guid, CancellationToken>(
                (_, _, phone, _, _, _, _, _, _) => capturedPhone = phone)
            .ReturnsAsync(100);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedPhone.Should().NotBeNull();
            capturedPhone!.PhoneNumber.Should().Be(model.Phone.PhoneNumber);
            capturedPhone.PhoneNumberTypeId.Should().Be(model.Phone.PhoneNumberTypeId);
        }
    }

    [Fact]
    public async Task Handle_creates_email_address_correctlyAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        var command = new CreateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        EmailAddressEntity? capturedEmail = null;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeEntity, PersonEntity, PersonPhone, EmailAddressEntity, AddressEntity, int, DateTime, Guid, CancellationToken>(
                (_, _, _, email, _, _, _, _, _) => capturedEmail = email)
            .ReturnsAsync(100);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEmail.Should().NotBeNull();
            capturedEmail!.EmailAddressName.Should().Be(model.EmailAddress);
        }
    }

    [Fact]
    public async Task Handle_maps_address_correctlyAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        var command = new CreateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        AddressEntity? capturedAddress = null;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeEntity, PersonEntity, PersonPhone, EmailAddressEntity, AddressEntity, int, DateTime, Guid, CancellationToken>(
                (_, _, _, _, address, _, _, _, _) => capturedAddress = address)
            .ReturnsAsync(100);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedAddress.Should().NotBeNull();
            capturedAddress!.AddressLine1.Should().Be(model.Address.AddressLine1);
            capturedAddress.AddressLine2.Should().Be(model.Address.AddressLine2);
            capturedAddress.City.Should().Be(model.Address.City);
            capturedAddress.PostalCode.Should().Be(model.Address.PostalCode);
            capturedAddress.StateProvinceId.Should().Be(model.Address.StateProvince!.Id);
        }
    }

    [Fact]
    public async Task Handle_passes_correct_parameters_to_repositoryAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var testGuid = Guid.NewGuid();

        var command = new CreateEmployeeCommand
        {
            Model = model,
            ModifiedDate = testDate,
            RowGuid = testGuid
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        int? capturedAddressTypeId = null;
        DateTime? capturedModifiedDate = null;
        Guid? capturedRowGuid = null;

        _mockEmployeeRepository
            .Setup(x => x.CreateEmployeeWithPersonAsync(
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeEntity, PersonEntity, PersonPhone, EmailAddressEntity, AddressEntity, int, DateTime, Guid, CancellationToken>(
                (_, _, _, _, _, addressTypeId, modifiedDate, rowGuid, _) =>
                {
                    capturedAddressTypeId = addressTypeId;
                    capturedModifiedDate = modifiedDate;
                    capturedRowGuid = rowGuid;
                })
            .ReturnsAsync(100);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedAddressTypeId.Should().Be(model.AddressTypeId);
            capturedModifiedDate.Should().Be(testDate);
            capturedRowGuid.Should().Be(testGuid);
        }
    }
}
