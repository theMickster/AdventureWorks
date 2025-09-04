using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fakes;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class TransferEmployeeDepartmentCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository = new();
    private readonly Mock<IShiftRepository> _mockShiftRepository = new();
    private readonly Mock<IValidator<EmployeeTransferModel>> _mockValidator = new();
    private readonly Mock<ILogger<TransferEmployeeDepartmentCommandHandler>> _mockLogger = new();
    private TransferEmployeeDepartmentCommandHandler? _sut;

    public TransferEmployeeDepartmentCommandHandlerTests()
    {
        _sut = new TransferEmployeeDepartmentCommandHandler(
            _mockEmployeeRepository.Object,
            _mockDepartmentRepository.Object,
            _mockShiftRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_throws_when_employeeRepository_is_null()
    {
        ((Action)(() => _ = new TransferEmployeeDepartmentCommandHandler(
            null!,
            _mockDepartmentRepository.Object,
            _mockShiftRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("employeeRepository");
    }

    [Fact]
    public void Constructor_throws_when_departmentRepository_is_null()
    {
        ((Action)(() => _ = new TransferEmployeeDepartmentCommandHandler(
            _mockEmployeeRepository.Object,
            null!,
            _mockShiftRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("departmentRepository");
    }

    [Fact]
    public void Constructor_throws_when_shiftRepository_is_null()
    {
        ((Action)(() => _ = new TransferEmployeeDepartmentCommandHandler(
            _mockEmployeeRepository.Object,
            _mockDepartmentRepository.Object,
            null!,
            _mockValidator.Object,
            _mockLogger.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("shiftRepository");
    }

    [Fact]
    public void Constructor_throws_when_validator_is_null()
    {
        ((Action)(() => _ = new TransferEmployeeDepartmentCommandHandler(
            _mockEmployeeRepository.Object,
            _mockDepartmentRepository.Object,
            _mockShiftRepository.Object,
            null!,
            _mockLogger.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("validator");
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        ((Action)(() => _ = new TransferEmployeeDepartmentCommandHandler(
            _mockEmployeeRepository.Object,
            _mockDepartmentRepository.Object,
            _mockShiftRepository.Object,
            _mockValidator.Object,
            null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        Func<Task> act = async () => await _sut!.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_when_request_model_is_nullAsync()
    {
        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = null!,
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_fails_validationAsync()
    {
        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 1, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        var fakeValidator = new FakeFailureValidator<EmployeeTransferModel>(
            "DepartmentId",
            "DepartmentId must be greater than 0.");

        _sut = new TransferEmployeeDepartmentCommandHandler(
            _mockEmployeeRepository.Object,
            _mockDepartmentRepository.Object,
            _mockShiftRepository.Object,
            fakeValidator,
            _mockLogger.Object);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_throws_key_not_found_when_employee_not_foundAsync()
    {
        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 999,
            Model = new EmployeeTransferModel { DepartmentId = 1, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public async Task Handle_throws_rule03_validation_exception_for_inactive_employeeAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetInactiveEmployeeEntity(businessEntityId: 1);
        employee.EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>();

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_throws_rule01_validation_exception_for_nonexistent_departmentAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 999, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DepartmentEntity?)null);

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Should().Contain(e => e.ErrorCode == "Rule-01");
    }

    [Fact]
    public async Task Handle_throws_rule02_validation_exception_for_nonexistent_shiftAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 2 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 3, Name = "Finance", GroupName = "Executive General" });

        _mockShiftRepository
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShiftEntity?)null);

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Should().Contain(e => e.ErrorCode == "Rule-02");
    }

    [Fact]
    public async Task Handle_throws_conflict_exception_when_no_active_assignmentAsync()
    {
        // Employee has history but all records have EndDate set (no active assignment)
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: false,
            hasPastTermination: true);
        employee.CurrentFlag = true; // Active employee but no open department record

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 3, Name = "Finance", GroupName = "Executive General" });

        _mockShiftRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftEntity { ShiftId = 1, Name = "Day" });

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_throws_rule04_validation_exception_for_same_dept_and_shiftAsync()
    {
        // The fixture active assignment has DepartmentId=2, ShiftId=2
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 2, ShiftId = 2 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 2, Name = "Tool Design", GroupName = "Research and Development" });

        _mockShiftRepository
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftEntity { ShiftId = 2, Name = "Evening" });

        Func<Task> act = async () => await _sut!.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Should().Contain(e => e.ErrorCode == "Rule-04" && e.PropertyName == "DepartmentAndShift");
    }

    [Fact]
    public async Task Handle_succeeds_when_same_department_different_shiftAsync()
    {
        // Active assignment: DepartmentId=2, ShiftId=2. Transfer to DepartmentId=2, ShiftId=1 — should NOT trigger Rule-04.
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 2, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 2, Name = "Tool Design", GroupName = "Research and Development" });

        _mockShiftRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftEntity { ShiftId = 1, Name = "Day" });

        _mockEmployeeRepository
            .Setup(x => x.TransferEmployeeDepartmentAsync(
                1, 2, 1, DefaultAuditDate.Date, DefaultAuditDate, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut!.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_succeeds_when_different_department_same_shiftAsync()
    {
        // Active assignment: DepartmentId=2, ShiftId=2. Transfer to DepartmentId=3, ShiftId=2 — should NOT trigger Rule-04.
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 2 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = DefaultAuditDate.Date
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 3, Name = "Finance", GroupName = "Executive General" });

        _mockShiftRepository
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftEntity { ShiftId = 2, Name = "Evening" });

        _mockEmployeeRepository
            .Setup(x => x.TransferEmployeeDepartmentAsync(
                1, 3, 2, DefaultAuditDate.Date, DefaultAuditDate, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut!.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_calls_transfer_repository_method_on_successAsync()
    {
        // Active assignment: DepartmentId=2, ShiftId=2. Transfer to DepartmentId=3, ShiftId=1.
        var employee = HumanResourcesDomainFixtures.GetEmployeeWithDepartmentHistory(
            businessEntityId: 1,
            hasActiveAssignment: true);

        var transferDate = DefaultAuditDate.Date;
        var command = new TransferEmployeeDepartmentCommand
        {
            EmployeeId = 1,
            Model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 1 },
            ModifiedDate = DefaultAuditDate,
            TransferDate = transferDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<EmployeeTransferModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithDepartmentHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 3, Name = "Finance", GroupName = "Executive General" });

        _mockShiftRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShiftEntity { ShiftId = 1, Name = "Day" });

        _mockEmployeeRepository
            .Setup(x => x.TransferEmployeeDepartmentAsync(
                1, 3, 1, transferDate, DefaultAuditDate, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut!.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(Unit.Value);

            _mockEmployeeRepository.Verify(
                x => x.TransferEmployeeDepartmentAsync(
                    1, 3, 1, transferDate, DefaultAuditDate, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
