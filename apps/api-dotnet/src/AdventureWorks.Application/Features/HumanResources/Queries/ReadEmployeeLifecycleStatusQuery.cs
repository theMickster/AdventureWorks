using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Query to retrieve aggregated employee lifecycle status.
/// Combines data from Employee, Person, EmployeeDepartmentHistory, and EmployeePayHistory.
/// Returns comprehensive status including current department, pay rate, and PTO balances.
/// </summary>
public sealed class ReadEmployeeLifecycleStatusQuery : IRequest<EmployeeLifecycleStatusModel?>
{
    /// <summary>
    /// Employee's BusinessEntityId to retrieve status for.
    /// </summary>
    public int EmployeeId { get; set; }
}
