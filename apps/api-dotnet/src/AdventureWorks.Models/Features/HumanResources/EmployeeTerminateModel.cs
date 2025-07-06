namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Input model for terminating an employee (offboarding workflow).
/// Orchestrates department assignment closure, CurrentFlag update, and PTO payout.
/// </summary>
public sealed class EmployeeTerminateModel
{
    /// <summary>
    /// Employee's BusinessEntityId to terminate.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Effective termination date. Cannot be more than 90 days in the future.
    /// Used to set EndDate on active EmployeeDepartmentHistory record.
    /// </summary>
    public DateTime TerminationDate { get; set; }

    /// <summary>
    /// Reason for termination (required for audit compliance).
    /// Maximum 500 characters.
    /// </summary>
    public required string Reason { get; set; }

    /// <summary>
    /// Termination type classification (required for HR reporting).
    /// Valid values: "Voluntary", "Involuntary", "Retirement", "Layoff".
    /// </summary>
    public required string TerminationType { get; set; }

    /// <summary>
    /// Whether the employee is eligible for rehire.
    /// Used for future validation in RehireEmployeeCommand.
    /// </summary>
    public bool EligibleForRehire { get; set; }

    /// <summary>
    /// Whether to calculate and payout remaining PTO balance.
    /// Default: true. When true, zeros out VacationHours and SickLeaveHours.
    /// </summary>
    public bool PayoutPto { get; set; } = true;

    /// <summary>
    /// Optional termination notes (e.g., "Exit interview completed", "Equipment returned").
    /// Maximum 1000 characters.
    /// </summary>
    public string? Notes { get; set; }
}
