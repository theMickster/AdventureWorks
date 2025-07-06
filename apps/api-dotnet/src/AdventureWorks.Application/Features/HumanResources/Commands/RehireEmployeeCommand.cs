using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to execute employee rehire workflow.
/// Orchestrates reactivation with new department assignment, pay rate, and PTO grants.
/// Validates 90-day minimum period since termination.
/// Returns the BusinessEntityId upon successful rehire.
/// </summary>
public sealed class RehireEmployeeCommand : IRequest<int>
{
    /// <summary>
    /// Rehire workflow input model containing new department, pay rate, and seniority options.
    /// </summary>
    public required EmployeeRehireModel Model { get; set; }

    /// <summary>
    /// System modification timestamp for audit trail.
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}
