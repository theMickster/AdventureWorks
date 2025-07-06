using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for HireEmployeeCommand.
/// Orchestrates employee hire workflow: department assignment, initial pay rate, and PTO grants.
/// </summary>
public sealed class HireEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IValidator<EmployeeHireModel> validator,
    ILogger<HireEmployeeCommandHandler> logger)
    : IRequestHandler<HireEmployeeCommand, int>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeeHireModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<HireEmployeeCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<int> Handle(HireEmployeeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var employee = await _employeeRepository.GetByIdAsync(request.Model.EmployeeId);
        if (employee == null)
        {
            _logger.LogError("Employee with ID {EmployeeId} not found", request.Model.EmployeeId);
            throw new KeyNotFoundException($"Employee with ID {request.Model.EmployeeId} not found.");
        }

        if (employee.CurrentFlag)
        {
            _logger.LogWarning(
                "Cannot hire employee {EmployeeId} - already active (CurrentFlag = true)",
                request.Model.EmployeeId);
            throw new InvalidOperationException($"Employee {request.Model.EmployeeId} is already active and cannot be hired again.");
        }

        employee.HireDate = request.Model.HireDate;
        employee.CurrentFlag = true;
        employee.VacationHours = request.Model.InitialVacationHours;
        employee.SickLeaveHours = request.Model.InitialSickLeaveHours;
        employee.ModifiedDate = request.ModifiedDate;

        _logger.LogInformation(
            "Updating employee {EmployeeId}: HireDate={HireDate}, VacationHours={VacationHours}, SickLeaveHours={SickLeaveHours}",
            employee.BusinessEntityId,
            request.Model.HireDate,
            request.Model.InitialVacationHours,
            request.Model.InitialSickLeaveHours);

        var deptHistory = new EmployeeDepartmentHistory
        {
            BusinessEntityId = employee.BusinessEntityId,
            DepartmentId = request.Model.DepartmentId,
            ShiftId = request.Model.ShiftId,
            StartDate = request.Model.HireDate,
            EndDate = null,
            ModifiedDate = request.ModifiedDate
        };

        employee.EmployeeDepartmentHistory ??= new List<EmployeeDepartmentHistory>();
        employee.EmployeeDepartmentHistory.Add(deptHistory);

        _logger.LogInformation(
            "Creating department assignment for employee {EmployeeId}: DepartmentId={DepartmentId}, ShiftId={ShiftId}",
            employee.BusinessEntityId,
            request.Model.DepartmentId,
            request.Model.ShiftId);

        var payHistory = new EmployeePayHistory
        {
            BusinessEntityId = employee.BusinessEntityId,
            RateChangeDate = request.Model.HireDate,
            Rate = request.Model.InitialPayRate,
            PayFrequency = request.Model.PayFrequency,
            ModifiedDate = request.ModifiedDate
        };

        employee.EmployeePayHistory ??= new List<EmployeePayHistory>();
        employee.EmployeePayHistory.Add(payHistory);

        _logger.LogInformation(
            "Creating initial pay rate for employee {EmployeeId}: Rate={Rate}, Frequency={Frequency}",
            employee.BusinessEntityId,
            request.Model.InitialPayRate,
            request.Model.PayFrequency);

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation(
            "Employee {EmployeeId} hired successfully on {HireDate} to department {DepartmentId}",
            employee.BusinessEntityId,
            request.Model.HireDate,
            request.Model.DepartmentId);

        //TODO: Publish domain event for integrations (email notifications, provisioning, etc.)
        // await _mediator.Publish(new EmployeeHiredEvent(employee.BusinessEntityId, request.Model.HireDate), cancellationToken);

        return employee.BusinessEntityId;
    }
}
