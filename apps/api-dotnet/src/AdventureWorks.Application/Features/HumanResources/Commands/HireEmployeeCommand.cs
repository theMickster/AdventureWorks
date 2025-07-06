using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to execute employee hire workflow (onboarding).
/// Orchestrates department assignment, initial pay rate, PTO grants, and manager assignment.
/// Returns the BusinessEntityId upon successful hire.
/// </summary>
public sealed class HireEmployeeCommand : IRequest<int>
{
    /// <summary>
    /// Hire workflow input model containing department, pay, and manager details.
    /// </summary>
    public required EmployeeHireModel Model { get; set; }

    /// <summary>
    /// System modification timestamp for audit trail.
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}
