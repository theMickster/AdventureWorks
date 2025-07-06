using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for TerminateEmployeeCommand.
/// Orchestrates employee termination workflow: CurrentFlag update, department closure, and PTO payout.
/// </summary>
public sealed class TerminateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IValidator<EmployeeTerminateModel> validator,
    ILogger<TerminateEmployeeCommandHandler> logger)
    : IRequestHandler<TerminateEmployeeCommand, Unit>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeeTerminateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<TerminateEmployeeCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var employee = await _employeeRepository.GetEmployeeByIdWithDepartmentHistoryAsync(
            request.Model.EmployeeId,
            cancellationToken);

        if (employee == null)
        {
            _logger.LogError("Employee with ID {EmployeeId} not found", request.Model.EmployeeId);
            throw new KeyNotFoundException($"Employee with ID {request.Model.EmployeeId} not found.");
        }

        if (!employee.CurrentFlag)
        {
            _logger.LogWarning(
                "Cannot terminate employee {EmployeeId} - already inactive (CurrentFlag = false)",
                request.Model.EmployeeId);
            throw new InvalidOperationException($"Employee {request.Model.EmployeeId} is already terminated.");
        }

        employee.CurrentFlag = false;
        employee.ModifiedDate = request.ModifiedDate;

        _logger.LogInformation(
            "Terminating employee {EmployeeId}: TerminationDate={TerminationDate}, Type={TerminationType}, Reason={Reason}",
            employee.BusinessEntityId,
            request.Model.TerminationDate,
            request.Model.TerminationType,
            request.Model.Reason);

        var activeDeptHistory = employee.EmployeeDepartmentHistory?
            .FirstOrDefault(dh => dh.EndDate == null);

        if (activeDeptHistory != null)
        {
            activeDeptHistory.EndDate = request.Model.TerminationDate;
            activeDeptHistory.ModifiedDate = request.ModifiedDate;

            _logger.LogInformation(
                "Ending department assignment for employee {EmployeeId}: DepartmentId={DepartmentId}, EndDate={EndDate}",
                employee.BusinessEntityId,
                activeDeptHistory.DepartmentId,
                request.Model.TerminationDate);
        }
        else
        {
            _logger.LogWarning(
                "No active department assignment found for employee {EmployeeId} during termination",
                employee.BusinessEntityId);
        }

        if (request.Model.PayoutPto)
        {
            var totalPtoHours = employee.VacationHours + employee.SickLeaveHours;

            _logger.LogInformation(
                "Employee {EmployeeId} termination PTO payout: {TotalHours} hours (Vacation: {VacationHours}, Sick: {SickLeaveHours})",
                employee.BusinessEntityId,
                totalPtoHours,
                employee.VacationHours,
                employee.SickLeaveHours);

            // TODO: Create PTO payout record in finance system integration
            // For now, we'll zero out balances to indicate payout processed
            employee.VacationHours = 0;
            employee.SickLeaveHours = 0;
        }

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation(
            "Employee {EmployeeId} terminated successfully on {TerminationDate}. Reason: {Reason}, Type: {Type}, EligibleForRehire: {EligibleForRehire}",
            employee.BusinessEntityId,
            request.Model.TerminationDate,
            request.Model.Reason,
            request.Model.TerminationType,
            request.Model.EligibleForRehire);

        //TODO: Publish domain event for integrations (offboarding tasks, access revocation, etc.)
        // await _mediator.Publish(new EmployeeTerminatedEvent(employee.BusinessEntityId, request.Model.TerminationDate), cancellationToken);

        return Unit.Value;
    }
}
