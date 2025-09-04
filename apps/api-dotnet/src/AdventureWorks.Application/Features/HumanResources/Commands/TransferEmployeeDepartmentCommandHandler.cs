using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>Handles <see cref="TransferEmployeeDepartmentCommand"/> by closing the active department assignment and opening a new one.</summary>
public sealed class TransferEmployeeDepartmentCommandHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IShiftRepository shiftRepository,
    IValidator<EmployeeTransferModel> validator,
    ILogger<TransferEmployeeDepartmentCommandHandler> logger)
    : IRequestHandler<TransferEmployeeDepartmentCommand, Unit>
{
    private readonly IEmployeeRepository _employeeRepository =
        employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly IShiftRepository _shiftRepository =
        shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
    private readonly IValidator<EmployeeTransferModel> _validator =
        validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<TransferEmployeeDepartmentCommandHandler> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(TransferEmployeeDepartmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var employee = await _employeeRepository.GetEmployeeByIdWithDepartmentHistoryAsync(
            request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            _logger.LogWarning("Employee {EmployeeId} not found for department transfer.", request.EmployeeId);
            throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found.");
        }

        if (!employee.CurrentFlag)
        {
            _logger.LogWarning(
                "Transfer rejected for employee {EmployeeId}. Rule={RuleCode}: {Reason}",
                request.EmployeeId, "Rule-03", "Employee is not active.");
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.EmployeeId),
                    "Employee is not active and cannot be transferred.")
                {
                    ErrorCode = "Rule-03"
                }
            });
        }

        var department = await _departmentRepository.GetByIdAsync((int)request.Model.DepartmentId, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning(
                "Transfer rejected for employee {EmployeeId}. Rule={RuleCode}: Department {DepartmentId} not found.",
                request.EmployeeId, "Rule-01", request.Model.DepartmentId);
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(EmployeeTransferModel.DepartmentId),
                    $"Department with ID {request.Model.DepartmentId} does not exist.")
                {
                    ErrorCode = "Rule-01"
                }
            });
        }

        var shift = await _shiftRepository.GetByIdAsync((int)request.Model.ShiftId, cancellationToken);
        if (shift is null)
        {
            _logger.LogWarning(
                "Transfer rejected for employee {EmployeeId}. Rule={RuleCode}: Shift {ShiftId} not found.",
                request.EmployeeId, "Rule-02", request.Model.ShiftId);
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(EmployeeTransferModel.ShiftId),
                    $"Shift with ID {request.Model.ShiftId} does not exist.")
                {
                    ErrorCode = "Rule-02"
                }
            });
        }

        var activeRecord = employee.EmployeeDepartmentHistory?.FirstOrDefault(dh => dh.EndDate == null);
        if (activeRecord is null)
        {
            _logger.LogWarning(
                "Transfer rejected for employee {EmployeeId}. ConflictCode={ConflictCode}: {Reason}",
                request.EmployeeId, "Conflict-01", "No active department assignment found.");
            throw new ConflictException($"Employee {request.EmployeeId} has no active department assignment to close.");
        }

        if (activeRecord.DepartmentId == request.Model.DepartmentId && activeRecord.ShiftId == request.Model.ShiftId)
        {
            _logger.LogWarning(
                "Transfer rejected for employee {EmployeeId}. Rule={RuleCode}: Already assigned to department {DepartmentId} shift {ShiftId}.",
                request.EmployeeId, "Rule-04", request.Model.DepartmentId, request.Model.ShiftId);
            throw new ValidationException(new[]
            {
                new ValidationFailure("DepartmentAndShift",
                    "Employee is already assigned to the specified department and shift.")
                {
                    ErrorCode = "Rule-04"
                }
            });
        }

        await _employeeRepository.TransferEmployeeDepartmentAsync(
            request.EmployeeId,
            request.Model.DepartmentId,
            request.Model.ShiftId,
            request.TransferDate,
            request.ModifiedDate,
            cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} transferred to department {DepartmentId} shift {ShiftId}.",
            request.EmployeeId, request.Model.DepartmentId, request.Model.ShiftId);

        return Unit.Value;
    }
}
