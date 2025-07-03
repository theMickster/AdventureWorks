using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class PatchEmployeeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeeUpdateModel>> _mockValidator = new();
    private PatchEmployeeCommandHandler _sut;

    public PatchEmployeeCommandHandlerTests()
    {
        _sut = new PatchEmployeeCommandHandler(
            _mockEmployeeRepository.Object,
            _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new PatchEmployeeCommandHandler(
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("employeeRepository");

            _ = ((Action)(() => _sut = new PatchEmployeeCommandHandler(
                    _mockEmployeeRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public void Handle_throws_exception_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_exception_when_patch_document_is_null()
    {
        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_employee_not_found()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "John");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 999,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_patched_model_is_invalid()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, ""); // Empty first name should fail validation

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeUpdateModel>(
            "FirstName",
            "First name is required");

        _sut = new PatchEmployeeCommandHandler(
            _mockEmployeeRepository.Object,
            validator);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetCompleteEmployeeEntity());

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "First name is required")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_successfully_applies_patch_and_updates_employee()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        employeeEntity.PersonBusinessEntity.FirstName = "John";
        employeeEntity.PersonBusinessEntity.LastName = "Doe";
        employeeEntity.MaritalStatus = "S";

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");
        patchDocument.Replace(x => x.MaritalStatus, "M");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            employeeEntity.PersonBusinessEntity.FirstName.Should().Be("Jane", "because the patch document replaced the first name");
            employeeEntity.PersonBusinessEntity.LastName.Should().Be("Doe", "because last name was not patched");
            employeeEntity.MaritalStatus.Should().Be("M", "because the patch document replaced the marital status");
            employeeEntity.ModifiedDate.Should().Be(DefaultAuditDate);
            employeeEntity.PersonBusinessEntity.ModifiedDate.Should().Be(DefaultAuditDate);
        }

        _mockEmployeeRepository.Verify(
            x => x.UpdateAsync(employeeEntity),
            Times.Once);
    }

    [Fact]
    public async Task Handle_applies_multiple_patch_operations_correctly()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        employeeEntity.PersonBusinessEntity.FirstName = "John";
        employeeEntity.PersonBusinessEntity.LastName = "Doe";
        employeeEntity.PersonBusinessEntity.MiddleName = "William";
        employeeEntity.PersonBusinessEntity.Title = "Mr.";
        employeeEntity.PersonBusinessEntity.Suffix = "Jr.";
        employeeEntity.MaritalStatus = "S";
        employeeEntity.Gender = "M";

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");
        patchDocument.Replace(x => x.MiddleName, null);
        patchDocument.Replace(x => x.Title, "Ms.");
        patchDocument.Replace(x => x.MaritalStatus, "M");
        patchDocument.Replace(x => x.Gender, "F");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            employeeEntity.PersonBusinessEntity.FirstName.Should().Be("Jane");
            employeeEntity.PersonBusinessEntity.LastName.Should().Be("Doe", "because last name was not patched");
            employeeEntity.PersonBusinessEntity.MiddleName.Should().BeNull();
            employeeEntity.PersonBusinessEntity.Title.Should().Be("Ms.");
            employeeEntity.PersonBusinessEntity.Suffix.Should().Be("Jr.", "because suffix was not patched");
            employeeEntity.MaritalStatus.Should().Be("M");
            employeeEntity.Gender.Should().Be("F");
            employeeEntity.ModifiedDate.Should().Be(DefaultAuditDate);
            employeeEntity.PersonBusinessEntity.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_returns_Unit_value_when_successful()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "UpdatedName");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_validates_patched_model_before_updating()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");
        patchDocument.Replace(x => x.LastName, "Smith");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<EmployeeUpdateModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockValidator.Verify(
            x => x.ValidateAsync(It.IsAny<ValidationContext<EmployeeUpdateModel>>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "because validation should occur after applying patch and before updating entity");
    }

    [Fact]
    public async Task Handle_ensures_Id_field_remains_immutable_when_patched()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        employeeEntity.BusinessEntityId = 100;

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.Id, 999); // Try to change the Id
        patchDocument.Replace(x => x.FirstName, "Jane");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            // The handler should restore the Id to its original value even if patched
            employeeEntity.BusinessEntityId.Should().Be(100, "because Id should remain immutable");
            employeeEntity.PersonBusinessEntity.FirstName.Should().Be("Jane", "because other fields should still be patched");
        }
    }

    [Fact]
    public async Task Handle_patches_all_PersonEntity_fields_correctly()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        employeeEntity.PersonBusinessEntity.FirstName = "Original";
        employeeEntity.PersonBusinessEntity.LastName = "Name";
        employeeEntity.PersonBusinessEntity.MiddleName = "Old";
        employeeEntity.PersonBusinessEntity.Title = "Mr.";
        employeeEntity.PersonBusinessEntity.Suffix = "Jr.";

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "NewFirst");
        patchDocument.Replace(x => x.LastName, "NewLast");
        patchDocument.Replace(x => x.MiddleName, "NewMiddle");
        patchDocument.Replace(x => x.Title, "Dr.");
        patchDocument.Replace(x => x.Suffix, "Sr.");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            employeeEntity.PersonBusinessEntity.FirstName.Should().Be("NewFirst");
            employeeEntity.PersonBusinessEntity.LastName.Should().Be("NewLast");
            employeeEntity.PersonBusinessEntity.MiddleName.Should().Be("NewMiddle");
            employeeEntity.PersonBusinessEntity.Title.Should().Be("Dr.");
            employeeEntity.PersonBusinessEntity.Suffix.Should().Be("Sr.");
        }
    }

    [Fact]
    public async Task Handle_patches_all_EmployeeEntity_fields_correctly()
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        employeeEntity.MaritalStatus = "S";
        employeeEntity.Gender = "M";

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.MaritalStatus, "M");
        patchDocument.Replace(x => x.Gender, "F");

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            employeeEntity.MaritalStatus.Should().Be("M");
            employeeEntity.Gender.Should().Be("F");
        }
    }

    [Theory]
    [InlineData("M")]
    [InlineData("S")]
    public async Task Handle_accepts_valid_marital_status_values(string maritalStatus)
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.MaritalStatus, maritalStatus);

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        employeeEntity.MaritalStatus.Should().Be(maritalStatus);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    public async Task Handle_accepts_valid_gender_values(string gender)
    {
        var employeeEntity = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();

        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.Gender, gender);

        var command = new PatchEmployeeCommand
        {
            EmployeeId = 100,
            PatchDocument = patchDocument,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employeeEntity);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        employeeEntity.Gender.Should().Be(gender);
    }
}
