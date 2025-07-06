namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Input model for rehiring a former employee.
/// Orchestrates reactivation with new department assignment, pay rate, and PTO grants.
/// </summary>
public sealed class EmployeeRehireModel
{
    /// <summary>
    /// Employee's BusinessEntityId (former employee with CurrentFlag = false).
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// New hire date for rehire. Cannot be in the past.
    /// Must be at least 90 days after previous termination date.
    /// </summary>
    public DateTime RehireDate { get; set; }

    /// <summary>
    /// New department assignment (references HumanResources.Department).
    /// </summary>
    public short DepartmentId { get; set; }

    /// <summary>
    /// New shift assignment: 1=Day, 2=Evening, 3=Night (references HumanResources.Shift).
    /// </summary>
    public byte ShiftId { get; set; }

    /// <summary>
    /// New pay rate (hourly or annual depending on SalariedFlag).
    /// Must be greater than 0 and less than or equal to $500.00.
    /// </summary>
    public decimal PayRate { get; set; }

    /// <summary>
    /// Pay frequency: 1=Monthly, 2=Bi-weekly.
    /// </summary>
    public byte PayFrequency { get; set; }

    /// <summary>
    /// New manager's BusinessEntityId for reporting relationship (optional).
    /// </summary>
    public int? ManagerId { get; set; }

    /// <summary>
    /// Whether to restore previous seniority for PTO calculations.
    /// If true, maintains existing VacationHours/SickLeaveHours balances.
    /// If false, resets to new hire amounts (40/24 hours).
    /// </summary>
    public bool RestoreSeniority { get; set; }

    /// <summary>
    /// Optional rehire notes (e.g., "Rehired with previous salary level", "Boomerang employee").
    /// Maximum 500 characters.
    /// </summary>
    public string? Notes { get; set; }
}
