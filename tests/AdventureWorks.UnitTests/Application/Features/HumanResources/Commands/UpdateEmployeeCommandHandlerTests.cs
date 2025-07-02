using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdateEmployeeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeeUpdateModel>> _mockValidator = new();
    private UpdateEmployeeCommandHandler _sut;

    public UpdateEmployeeCommandHandlerTests()
    {
        _sut = new UpdateEmployeeCommandHandler(_mockEmployeeRepository.Object, _mockValidator.Object);
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
    public async Task Handle_throws_exception_when_request_model_is_null()
    {
        var command = new UpdateEmployeeCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_is_invalid()
    {
        var command = new UpdateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeUpdateModel>(
            "FirstName",
            "First name is required");

        _sut = new UpdateEmployeeCommandHandler(_mockEmployeeRepository.Object, validator);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "First name is required")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_employee_not_found()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);
        var command = new UpdateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 100 not found.");
    }

    [Fact]
    public async Task Handle_updates_employee_and_person_fields_correctly()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);
        var employeeEntity = HumanResourcesDomainFixtures.GetValidEmployeeEntity(100);
        var personEntity = HumanResourcesDomainFixtures.GetValidPersonEntity(100);
        employeeEntity.PersonBusinessEntity = personEntity;

        var command = new UpdateEmployeeCommand
        {
            Model = model,
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
            personEntity.FirstName.Should().Be(model.FirstName);
            personEntity.LastName.Should().Be(model.LastName);
            personEntity.MiddleName.Should().Be(model.MiddleName);
            personEntity.Title.Should().Be(model.Title);
            personEntity.Suffix.Should().Be(model.Suffix);
            personEntity.ModifiedDate.Should().Be(DefaultAuditDate);

            employeeEntity.MaritalStatus.Should().Be(model.MaritalStatus);
            employeeEntity.Gender.Should().Be(model.Gender);
            employeeEntity.ModifiedDate.Should().Be(DefaultAuditDate);
        }

        _mockEmployeeRepository.Verify(
            x => x.UpdateAsync(employeeEntity),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_Unit_value_when_successful()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);
        var employeeEntity = HumanResourcesDomainFixtures.GetValidEmployeeEntity(100);
        var personEntity = HumanResourcesDomainFixtures.GetValidPersonEntity(100);
        employeeEntity.PersonBusinessEntity = personEntity;

        var command = new UpdateEmployeeCommand
        {
            Model = model,
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
}
