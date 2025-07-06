using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to execute employee termination workflow (offboarding).
/// Orchestrates CurrentFlag update, department assignment closure, and PTO payout.
/// Returns Unit (void) upon successful termination.
/// </summary>
public sealed class TerminateEmployeeCommand : IRequest<Unit>
{
    /// <summary>
    /// Termination workflow input model containing termination reason, type, and options.
    /// </summary>
    public required EmployeeTerminateModel Model { get; set; }

    /// <summary>
    /// System modification timestamp for audit trail.
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}
