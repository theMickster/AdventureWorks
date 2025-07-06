using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Handler for ReadEmployeeLifecycleStatusQuery.
/// Aggregates employee lifecycle data from Employee, Person, EmployeeDepartmentHistory, and EmployeePayHistory.
/// </summary>
public sealed class ReadEmployeeLifecycleStatusQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<ReadEmployeeLifecycleStatusQueryHandler> logger)
    : IRequestHandler<ReadEmployeeLifecycleStatusQuery, EmployeeLifecycleStatusModel?>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly ILogger<ReadEmployeeLifecycleStatusQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<EmployeeLifecycleStatusModel?> Handle(
        ReadEmployeeLifecycleStatusQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Retrieving lifecycle status for employee {EmployeeId}", request.EmployeeId);

        var employee = await _employeeRepository.GetEmployeeByIdWithLifecycleDataAsync(
            request.EmployeeId,
            cancellationToken);

        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} not found", request.EmployeeId);
            return null;
        }

        var activeDept = employee.EmployeeDepartmentHistory?
            .FirstOrDefault(dh => dh.EndDate == null);

        var currentPayRate = employee.EmployeePayHistory?
            .OrderByDescending(ph => ph.RateChangeDate)
            .FirstOrDefault();

        var terminationHistory = employee.EmployeeDepartmentHistory?
            .Where(dh => dh.EndDate.HasValue)
            .ToList();

        var terminationCount = terminationHistory?.Count ?? 0;

        var mostRecentTermination = terminationHistory?
            .OrderByDescending(dh => dh.EndDate)
            .FirstOrDefault();

        var employmentStatus = DetermineEmploymentStatus(employee);

        var daysEmployed = CalculateDaysEmployed(employee, mostRecentTermination);

        var status = new EmployeeLifecycleStatusModel
        {
            EmployeeId = employee.BusinessEntityId,
            FullName = $"{employee.PersonBusinessEntity.FirstName} {employee.PersonBusinessEntity.LastName}",
            EmploymentStatus = employmentStatus,
            HireDate = employee.HireDate,
            TerminationDate = mostRecentTermination?.EndDate,
            DaysEmployed = daysEmployed,
            CurrentDepartment = activeDept?.Department?.Name,
            CurrentShift = activeDept?.Shift?.Name,
            DepartmentStartDate = activeDept?.StartDate,
            CurrentPayRate = currentPayRate?.Rate,
            PayRateEffectiveDate = currentPayRate?.RateChangeDate,
            VacationHoursBalance = employee.VacationHours,
            SickLeaveHoursBalance = employee.SickLeaveHours,
            EligibleForRehire = !employee.CurrentFlag && terminationCount > 0,
            RehireCount = terminationCount
        };

        _logger.LogInformation(
            "Successfully retrieved lifecycle status for employee {EmployeeId}: Status={Status}, Department={Department}, PayRate={PayRate}",
            employee.BusinessEntityId,
            employmentStatus,
            activeDept?.Department?.Name ?? "None",
            currentPayRate?.Rate);

        return status;
    }

    private static string DetermineEmploymentStatus(Domain.Entities.HumanResources.EmployeeEntity employee)
    {
        return employee.CurrentFlag ? "Active" : "Terminated";
    }

    private static int? CalculateDaysEmployed(
        Domain.Entities.HumanResources.EmployeeEntity employee,
        Domain.Entities.HumanResources.EmployeeDepartmentHistory? mostRecentTermination)
    {
        if (employee.CurrentFlag)
        {
            return (DateTime.UtcNow - employee.HireDate).Days;
        }

        if (mostRecentTermination?.EndDate != null)
        {
            return (mostRecentTermination.EndDate.Value - employee.HireDate).Days;
        }

        return null;
    }
}
