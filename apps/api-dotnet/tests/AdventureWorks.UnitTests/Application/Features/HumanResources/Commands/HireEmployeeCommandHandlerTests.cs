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
public sealed class HireEmployeeCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IValidator<EmployeeHireModel>> _mockValidator = new();
    private readonly Mock<ILogger<HireEmployeeCommandHandler>> _mockLogger = new();
    private HireEmployeeCommandHandler _sut;

    public HireEmployeeCommandHandlerTests()
    {
        _sut = new HireEmployeeCommandHandler(
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
        var command = new HireEmployeeCommand
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
        var command = new HireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<EmployeeHireModel>(
            "InitialPayRate",
            "PayRate cannot exceed $500.00.");

        _sut = new HireEmployeeCommandHandler(
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
        var command = new HireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 999),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public async Task Handle_throws_invalid_operation_exception_when_employee_is_already_activeAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetValidEmployeeEntity(businessEntityId: 1);
        employee.CurrentFlag = true; // Already active

        var command = new HireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(employee);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee 1 is already active and cannot be hired again.");
    }

    [Fact]
    public async Task Handle_returns_business_entity_id_when_successfulAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        const int expectedBusinessEntityId = 1;

        var command = new HireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: expectedBusinessEntityId),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(expectedBusinessEntityId))
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
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);
        var testDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        model.HireDate = testDate;
        model.InitialVacationHours = 80;
        model.InitialSickLeaveHours = 48;

        var command = new HireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
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
            capturedEmployee!.HireDate.Should().Be(testDate);
            capturedEmployee.CurrentFlag.Should().BeTrue();
            capturedEmployee.VacationHours.Should().Be(80);
            capturedEmployee.SickLeaveHours.Should().Be(48);
            capturedEmployee.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_creates_department_history_record_correctlyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);
        model.DepartmentId = 5;
        model.ShiftId = 2;

        var command = new HireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
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

            var deptHistory = capturedEmployee.EmployeeDepartmentHistory!.First();
            deptHistory.BusinessEntityId.Should().Be(1);
            deptHistory.DepartmentId.Should().Be(5);
            deptHistory.ShiftId.Should().Be(2);
            deptHistory.StartDate.Should().Be(model.HireDate);
            deptHistory.EndDate.Should().BeNull(); // Active assignment
            deptHistory.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_creates_pay_history_record_correctlyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);
        model.InitialPayRate = 75.50m;
        model.PayFrequency = 1; // Monthly

        var command = new HireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
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
            capturedEmployee.EmployeePayHistory.Should().HaveCount(1);

            var payHistory = capturedEmployee.EmployeePayHistory!.First();
            payHistory.BusinessEntityId.Should().Be(1);
            payHistory.RateChangeDate.Should().Be(model.HireDate);
            payHistory.Rate.Should().Be(75.50m);
            payHistory.PayFrequency.Should().Be(1);
            payHistory.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }

    [Fact]
    public async Task Handle_initializes_collections_when_nullAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        employee.EmployeeDepartmentHistory = null;
        employee.EmployeePayHistory = null;

        var command = new HireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
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
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);

        var command = new HireEmployeeCommand
        {
            Model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
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
    public async Task Handle_orchestrates_complete_hire_workflow_successfullyAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        var model = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);
        var hireDate = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        model.HireDate = hireDate;
        model.DepartmentId = 3;
        model.ShiftId = 2;
        model.InitialPayRate = 65.00m;
        model.PayFrequency = 2;
        model.InitialVacationHours = 80;
        model.InitialSickLeaveHours = 40;

        var command = new HireEmployeeCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeHireModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(1))
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
            capturedEmployee!.HireDate.Should().Be(hireDate);
            capturedEmployee.CurrentFlag.Should().BeTrue();
            capturedEmployee.VacationHours.Should().Be(80);
            capturedEmployee.SickLeaveHours.Should().Be(40);
            capturedEmployee.ModifiedDate.Should().Be(DefaultAuditDate);

            // Verify department history
            capturedEmployee.EmployeeDepartmentHistory.Should().NotBeNull();
            capturedEmployee.EmployeeDepartmentHistory.Should().HaveCount(1);
            var deptHistory = capturedEmployee.EmployeeDepartmentHistory!.First();
            deptHistory.DepartmentId.Should().Be(3);
            deptHistory.ShiftId.Should().Be(2);
            deptHistory.StartDate.Should().Be(hireDate);
            deptHistory.EndDate.Should().BeNull();

            // Verify pay history
            capturedEmployee.EmployeePayHistory.Should().NotBeNull();
            capturedEmployee.EmployeePayHistory.Should().HaveCount(1);
            var payHistory = capturedEmployee.EmployeePayHistory!.First();
            payHistory.Rate.Should().Be(65.00m);
            payHistory.PayFrequency.Should().Be(2);
            payHistory.RateChangeDate.Should().Be(hireDate);

            // Verify repository was called
            _mockEmployeeRepository.Verify(x => x.UpdateAsync(It.IsAny<EmployeeEntity>()), Times.Once);
        }
    }
}
