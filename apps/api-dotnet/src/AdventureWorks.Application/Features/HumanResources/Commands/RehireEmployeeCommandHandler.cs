using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Constants;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for RehireEmployeeCommand.
/// Orchestrates employee rehire workflow: reactivation, new department assignment, pay rate, and PTO.
/// Validates 90-day minimum period since termination.
/// </summary>
public sealed class RehireEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IValidator<EmployeeRehireModel> validator,
    ILogger<RehireEmployeeCommandHandler> logger)
    : IRequestHandler<RehireEmployeeCommand, int>
{

    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeeRehireModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<RehireEmployeeCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<int> Handle(RehireEmployeeCommand request, CancellationToken cancellationToken)
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

        if (employee.CurrentFlag)
        {
            _logger.LogWarning(
                "Cannot rehire employee {EmployeeId} - already active (CurrentFlag = true). Use transfer instead.",
                request.Model.EmployeeId);
            throw new InvalidOperationException($"Employee {request.Model.EmployeeId} is already active. Use department transfer instead of rehire.");
        }

        var lastTermination = employee.EmployeeDepartmentHistory?
            .Where(dh => dh.EndDate.HasValue)
            .OrderByDescending(dh => dh.EndDate)
            .FirstOrDefault();

        if (lastTermination?.EndDate != null)
        {
            var daysSinceTermination = (request.Model.RehireDate - lastTermination.EndDate.Value).Days;
            if (daysSinceTermination < HumanResourcesConstants.MinimumDaysBeforeRehire)
            {
                _logger.LogWarning(
                    "Cannot rehire employee {EmployeeId} - only {DaysSinceTermination} days since termination. Minimum: {MinimumDays} days. Last termination: {TerminationDate}",
                    request.Model.EmployeeId,
                    daysSinceTermination,
                    HumanResourcesConstants.MinimumDaysBeforeRehire,
                    lastTermination.EndDate.Value);

                throw new InvalidOperationException(
                    $"Employee {request.Model.EmployeeId} cannot be rehired until {HumanResourcesConstants.MinimumDaysBeforeRehire} days after termination. " +
                    $"Last termination: {lastTermination.EndDate.Value:yyyy-MM-dd}. " +
                    $"Earliest rehire date: {lastTermination.EndDate.Value.AddDays(HumanResourcesConstants.MinimumDaysBeforeRehire):yyyy-MM-dd}");
            }

            _logger.LogInformation(
                "Employee {EmployeeId} rehire validation passed: {DaysSinceTermination} days since last termination on {TerminationDate}",
                employee.BusinessEntityId,
                daysSinceTermination,
                lastTermination.EndDate.Value);
        }

        employee.HireDate = request.Model.RehireDate; 
        employee.CurrentFlag = true;
        employee.ModifiedDate = request.ModifiedDate;

        _logger.LogInformation(
            "Rehiring employee {EmployeeId}: RehireDate={RehireDate}, RestoreSeniority={RestoreSeniority}",
            employee.BusinessEntityId,
            request.Model.RehireDate,
            request.Model.RestoreSeniority);

        if (request.Model.RestoreSeniority)
        {
            _logger.LogInformation(
                "Restoring seniority for employee {EmployeeId}: VacationHours={VacationHours}, SickLeaveHours={SickLeaveHours}",
                employee.BusinessEntityId,
                employee.VacationHours,
                employee.SickLeaveHours);
        }
        else
        {
            employee.VacationHours = HumanResourcesConstants.DefaultVacationHours;
            employee.SickLeaveHours = HumanResourcesConstants.DefaultSickLeaveHours;

            _logger.LogInformation(
                "Granting new hire PTO for employee {EmployeeId}: VacationHours={VacationHours}, SickLeaveHours={SickLeaveHours}",
                employee.BusinessEntityId,
                HumanResourcesConstants.DefaultVacationHours,
                HumanResourcesConstants.DefaultSickLeaveHours);
        }

        var deptHistory = new EmployeeDepartmentHistory
        {
            BusinessEntityId = employee.BusinessEntityId,
            DepartmentId = request.Model.DepartmentId,
            ShiftId = request.Model.ShiftId,
            StartDate = request.Model.RehireDate,
            EndDate = null, 
            ModifiedDate = request.ModifiedDate
        };

        employee.EmployeeDepartmentHistory ??= new List<EmployeeDepartmentHistory>();
        employee.EmployeeDepartmentHistory.Add(deptHistory);

        _logger.LogInformation(
            "Creating new department assignment for rehired employee {EmployeeId}: DepartmentId={DepartmentId}, ShiftId={ShiftId}",
            employee.BusinessEntityId,
            request.Model.DepartmentId,
            request.Model.ShiftId);

        var payHistory = new EmployeePayHistory
        {
            BusinessEntityId = employee.BusinessEntityId,
            RateChangeDate = request.Model.RehireDate,
            Rate = request.Model.PayRate,
            PayFrequency = request.Model.PayFrequency,
            ModifiedDate = request.ModifiedDate
        };

        employee.EmployeePayHistory ??= new List<EmployeePayHistory>();
        employee.EmployeePayHistory.Add(payHistory);

        _logger.LogInformation(
            "Creating new pay rate for rehired employee {EmployeeId}: Rate={Rate}, Frequency={Frequency}",
            employee.BusinessEntityId,
            request.Model.PayRate,
            request.Model.PayFrequency);

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation(
            "Employee {EmployeeId} rehired successfully on {RehireDate} to department {DepartmentId}",
            employee.BusinessEntityId,
            request.Model.RehireDate,
            request.Model.DepartmentId);

        // TODO: Publish domain event for integrations (re-provisioning, welcome back email, etc.)
        // await _mediator.Publish(new EmployeeRehiredEvent(employee.BusinessEntityId, request.Model.RehireDate), cancellationToken);

        return employee.BusinessEntityId;
    }
}
