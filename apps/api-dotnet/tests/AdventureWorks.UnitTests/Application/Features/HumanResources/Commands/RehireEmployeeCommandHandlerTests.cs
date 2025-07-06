using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class RehireEmployeeCommandHandlerTests : UnitTestBase
{
    private const int MinimumDaysSinceTermination = 90;

    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeeRehireModel>> _mockValidator = new();
    private readonly Mock<ILogger<RehireEmployeeCommandHandler>> _mockLogger = new();
    private RehireEmployeeCommandHandler _sut;

    public RehireEmployeeCommandHandlerTests()
    {
        _sut = new RehireEmployeeCommandHandler(
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
        var command = new RehireEmployeeCommand
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
        var command = new RehireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeRehireModel>(
            "PayRate",
            "PayRate cannot exceed $500.00.");

        _sut = new RehireEmployeeCommandHandler(
            _mockEmployeeRepository.Object,
            validator,
            _mockLogger.Object);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "PayRate cannot exceed $500.00.")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_exception_when_employee_not_foundAsync()
    {
        var command = new RehireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 999),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public async Task Handle_throws_invalid_operation_exception_when_employee_is_already_activeAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);
        employee.CurrentFlag = true; // Already active

        var command = new RehireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee 1 is already active. Use department transfer instead of rehire.");
    }

    [Fact]
    public async Task Handle_throws_invalid_operation_exception_when_rehire_within_90_daysAsync()
    {
        var terminationDate = DateTime.UtcNow.Date.AddDays(-50); // 50 days ago (< 90)
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: false);
        employee.CurrentFlag = false;

        // Add terminated department history
        employee.EmployeeDepartmentHistory!.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = new DateTime(2020, 1, 1),
            EndDate = terminationDate,
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RehireDate = DateTime.UtcNow.Date; // Today

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee 1 cannot be rehired until 90 days after termination.*");
    }

    [Fact]
    public async Task Handle_succeeds_when_rehire_is_exactly_90_days_after_terminationAsync()
    {
        var terminationDate = DateTime.UtcNow.Date.AddDays(-90);
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: false);
        employee.CurrentFlag = false;

        employee.EmployeeDepartmentHistory!.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = new DateTime(2020, 1, 1),
            EndDate = terminationDate,
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RehireDate = DateTime.UtcNow.Date;

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(1);
    }

    [Fact]
    public async Task Handle_succeeds_when_rehire_is_more_than_90_days_after_terminationAsync()
    {
        var terminationDate = DateTime.UtcNow.Date.AddDays(-120); // 120 days ago (> 90)
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: false);
        employee.CurrentFlag = false;

        employee.EmployeeDepartmentHistory!.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = new DateTime(2020, 1, 1),
            EndDate = terminationDate,
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RehireDate = DateTime.UtcNow.Date;

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_business_entity_id_when_successfulAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;

        const int expectedBusinessEntityId = 1;

        var command = new RehireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: expectedBusinessEntityId),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(expectedBusinessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockEmployeeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(expectedBusinessEntityId);
    }

    [Fact]
    public async Task Handle_updates_employee_core_fields_correctlyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;

        var rehireDate = DateTime.UtcNow.Date.AddDays(7);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RehireDate = rehireDate;

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            capturedEmployee!.HireDate.Should().Be(rehireDate);
            capturedEmployee.CurrentFlag.Should().BeTrue();
            capturedEmployee.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_grants_new_hire_pto_when_restore_seniority_is_falseAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;
        employee.VacationHours = 120; // Existing balance
        employee.SickLeaveHours = 80;  // Existing balance

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RestoreSeniority = false; // Grant new hire PTO

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            capturedEmployee!.VacationHours.Should().Be(40); // New hire amount
            capturedEmployee.SickLeaveHours.Should().Be(24); // New hire amount
        }
    }

    [Fact]
    public async Task Handle_preserves_existing_pto_when_restore_seniority_is_trueAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;
        employee.VacationHours = 120; // Existing balance
        employee.SickLeaveHours = 80;  // Existing balance

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RestoreSeniority = true; // Preserve existing PTO

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            capturedEmployee!.VacationHours.Should().Be(120); // Preserved
            capturedEmployee.SickLeaveHours.Should().Be(80);  // Preserved
        }
    }

    [Fact]
    public async Task Handle_creates_new_department_history_record_correctlyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;

        var existingHistoryCount = employee.EmployeeDepartmentHistory!.Count;

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.DepartmentId = 7;
        model.ShiftId = 3;

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            capturedEmployee.EmployeeDepartmentHistory.Should().HaveCount(existingHistoryCount + 1);

            var newDeptHistory = capturedEmployee.EmployeeDepartmentHistory!.Last();
            newDeptHistory.BusinessEntityId.Should().Be(1);
            newDeptHistory.DepartmentId.Should().Be(7);
            newDeptHistory.ShiftId.Should().Be(3);
            newDeptHistory.StartDate.Should().Be(model.RehireDate);
            newDeptHistory.EndDate.Should().BeNull(); // Active assignment
            newDeptHistory.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_creates_new_pay_history_record_correctlyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;

        var existingPayHistoryCount = employee.EmployeePayHistory!.Count;

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.PayRate = 85.75m;
        model.PayFrequency = 1; // Monthly

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            capturedEmployee!.EmployeePayHistory.Should().NotBeNull();
            capturedEmployee.EmployeePayHistory.Should().HaveCount(existingPayHistoryCount + 1);

            var newPayHistory = capturedEmployee.EmployeePayHistory!.Last();
            newPayHistory.BusinessEntityId.Should().Be(1);
            newPayHistory.RateChangeDate.Should().Be(model.RehireDate);
            newPayHistory.Rate.Should().Be(85.75m);
            newPayHistory.PayFrequency.Should().Be(1);
            newPayHistory.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_initializes_collections_when_nullAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        employee.EmployeeDepartmentHistory = null;
        employee.EmployeePayHistory = null;

        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RehireDate = DateTime.UtcNow.Date.AddDays(7); // Future date (no termination to validate)

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            capturedEmployee.EmployeeDepartmentHistory.Should().HaveCount(1);
            capturedEmployee.EmployeePayHistory.Should().NotBeNull();
            capturedEmployee.EmployeePayHistory.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task Handle_calls_repository_update_async_with_correct_employeeAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;

        var command = new RehireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
                e.CurrentFlag == true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_orchestrates_complete_rehire_workflow_with_seniority_restoration_successfullyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = false;
        employee.VacationHours = 160;
        employee.SickLeaveHours = 96;

        var existingDeptHistoryCount = employee.EmployeeDepartmentHistory!.Count;
        var existingPayHistoryCount = employee.EmployeePayHistory!.Count;

        var rehireDate = DateTime.UtcNow.Date.AddDays(7);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        model.RehireDate = rehireDate;
        model.DepartmentId = 5;
        model.ShiftId = 2;
        model.PayRate = 95.00m;
        model.PayFrequency = 2;
        model.RestoreSeniority = true; // Preserve PTO balances

        var command = new RehireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeRehireModel>(), It.IsAny<CancellationToken>()))
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
            result.Should().Be(1);

            // Verify employee core fields
            capturedEmployee.Should().NotBeNull();
            capturedEmployee!.HireDate.Should().Be(rehireDate);
            capturedEmployee.CurrentFlag.Should().BeTrue();
            capturedEmployee.ModifiedDate.Should().Be(DefaultAuditDate);

            // Verify PTO seniority restoration
            capturedEmployee.VacationHours.Should().Be(160); // Preserved
            capturedEmployee.SickLeaveHours.Should().Be(96);  // Preserved

            // Verify new department history
            capturedEmployee.EmployeeDepartmentHistory.Should().HaveCount(existingDeptHistoryCount + 1);
            var newDeptHistory = capturedEmployee.EmployeeDepartmentHistory!.Last();
            newDeptHistory.DepartmentId.Should().Be(5);
            newDeptHistory.ShiftId.Should().Be(2);
            newDeptHistory.StartDate.Should().Be(rehireDate);
            newDeptHistory.EndDate.Should().BeNull();

            // Verify new pay history
            capturedEmployee.EmployeePayHistory.Should().HaveCount(existingPayHistoryCount + 1);
            var newPayHistory = capturedEmployee.EmployeePayHistory!.Last();
            newPayHistory.Rate.Should().Be(95.00m);
            newPayHistory.PayFrequency.Should().Be(2);
            newPayHistory.RateChangeDate.Should().Be(rehireDate);

            // Verify repository was called
            _mockEmployeeRepository.Verify(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()), Times.Once);
        }
    }
}
