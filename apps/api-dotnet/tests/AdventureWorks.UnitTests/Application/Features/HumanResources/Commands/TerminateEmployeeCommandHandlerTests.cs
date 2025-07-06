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
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class TerminateEmployeeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeeTerminateModel>> _mockValidator = new();
    private readonly Mock<ILogger<TerminateEmployeeCommandHandler>> _mockLogger = new();
    private TerminateEmployeeCommandHandler _sut;

    public TerminateEmployeeCommandHandlerTests()
    {
        _sut = new TerminateEmployeeCommandHandler(
            _mockEmployeeRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object);
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
        var command = new TerminateEmployeeCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_is_invalidAsync()
    {
        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeTerminateModel>(
            "Reason",
            "Reason is required and cannot be empty.");

        _sut = new TerminateEmployeeCommandHandler(
            _mockEmployeeRepository.Object,
            validator,
            _mockLogger.Object);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "Reason is required and cannot be empty.")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_exception_when_employee_not_foundAsync()
    {
        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 999),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public async Task Handle_throws_invalid_operation_exception_when_employee_is_already_inactiveAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false);
        employee.CurrentFlag = false; // Already inactive

        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee 1 is already terminated.");
    }

    [Fact]
    public async Task Handle_returns_unit_value_when_successfulAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_sets_current_flag_to_false_when_terminatingAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);
        employee.CurrentFlag = true;

        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        EmployeeEntity? capturedEmployee = null;

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Callback<EmployeeEntity>(emp => capturedEmployee = emp)
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.CurrentFlag.Should().BeFalse();
            capturedEmployee.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_ends_active_department_assignment_correctlyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var terminationDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);
        model.TerminationDate = terminationDate;

        var command = new TerminateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        EmployeeEntity? capturedEmployee = null;

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Callback<EmployeeEntity>(emp => capturedEmployee = emp)
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.EmployeeDepartmentHistory.Should().NotBeNull();

            var activeDept = capturedEmployee.EmployeeDepartmentHistory!.FirstOrDefault(dh => dh.EndDate != null);
            activeDept.Should().NotBeNull();
            activeDept!.EndDate.Should().Be(terminationDate);
            activeDept.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_completes_successfully_when_no_active_department_assignment_existsAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false);
        employee.CurrentFlag = true; // Active but no department assignment

        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_zeros_pto_balances_when_payout_requested_async()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);
        employee.VacationHours = 120;
        employee.SickLeaveHours = 60;

        var model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);
        model.PayoutPto = true;

        var command = new TerminateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        EmployeeEntity? capturedEmployee = null;

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Callback<EmployeeEntity>(emp => capturedEmployee = emp)
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.VacationHours.Should().Be(0);
            capturedEmployee.SickLeaveHours.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_preserves_pto_balances_when_payout_not_requested_async()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);
        employee.VacationHours = 120;
        employee.SickLeaveHours = 60;

        var model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);
        model.PayoutPto = false;

        var command = new TerminateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        EmployeeEntity? capturedEmployee = null;

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Callback<EmployeeEntity>(emp => capturedEmployee = emp)
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.VacationHours.Should().Be(120);
            capturedEmployee.SickLeaveHours.Should().Be(60);
        }
    }

    [Fact]
    public async Task Handle_calls_repository_update_async_with_correct_employeeAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TerminateEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockEmployeeRepository.Verify(
            x => x.UpdateAsync(It.Is<EmployeeEntity>(e =>
                e.BusinessEntityId == 1 &&
                e.CurrentFlag == false)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_orchestrates_complete_termination_workflow_successfullyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);
        employee.VacationHours = 100;
        employee.SickLeaveHours = 50;

        var terminationDate = new DateTime(2024, 11, 30, 0, 0, 0, DateTimeKind.Utc);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);
        model.TerminationDate = terminationDate;
        model.TerminationType = "Voluntary";
        model.Reason = "Accepted position elsewhere";
        model.PayoutPto = true;

        var command = new TerminateEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTerminateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        EmployeeEntity? capturedEmployee = null;

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Callback<EmployeeEntity>(emp => capturedEmployee = emp)
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            // Verify return value
            result.Should().Be(Unit.Value);

            // Verify employee core fields
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.CurrentFlag.Should().BeFalse();
            capturedEmployee.ModifiedDate.Should().Be(DefaultAuditDate);

            // Verify department assignment ended
            var activeDept = capturedEmployee.EmployeeDepartmentHistory!.FirstOrDefault(dh => dh.EndDate != null);
            activeDept.Should().NotBeNull();
            activeDept!.EndDate.Should().Be(terminationDate);

            // Verify PTO payout (balances zeroed)
            capturedEmployee.VacationHours.Should().Be(0);
            capturedEmployee.SickLeaveHours.Should().Be(0);

            // Verify repository was called
            _mockEmployeeRepository.Verify(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()), Times.Once);
        }
    }
}
