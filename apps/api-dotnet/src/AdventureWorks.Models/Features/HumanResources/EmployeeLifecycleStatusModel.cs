namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Output model representing aggregated employee lifecycle status.
/// Combines data from Employee, Person, EmployeeDepartmentHistory, and EmployeePayHistory.
/// </summary>
public sealed class EmployeeLifecycleStatusModel
{
    /// <summary>
    /// Employee's BusinessEntityId.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Employee's full name (FirstName + LastName from Person table).
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Current employment status: "Active", "Terminated", "OnLeave".
    /// Derived from Employee.CurrentFlag.
    /// </summary>
    public required string EmploymentStatus { get; set; }

    /// <summary>
    /// Original hire date or most recent rehire date.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Termination date if employee has been terminated (null if active).
    /// Derived from most recent EmployeeDepartmentHistory.EndDate.
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Total days employed (from HireDate to TerminationDate or current date).
    /// </summary>
    public int? DaysEmployed { get; set; }

    /// <summary>
    /// Current department name (null if terminated).
    /// From active EmployeeDepartmentHistory record (WHERE EndDate IS NULL).
    /// </summary>
    public string? CurrentDepartment { get; set; }

    /// <summary>
    /// Current shift name (null if terminated).
    /// From active EmployeeDepartmentHistory record (WHERE EndDate IS NULL).
    /// </summary>
    public string? CurrentShift { get; set; }

    /// <summary>
    /// Start date of current department assignment.
    /// </summary>
    public DateTime? DepartmentStartDate { get; set; }

    /// <summary>
    /// Current pay rate (most recent EmployeePayHistory.Rate).
    /// </summary>
    public decimal? CurrentPayRate { get; set; }

    /// <summary>
    /// Effective date of current pay rate (most recent EmployeePayHistory.RateChangeDate).
    /// </summary>
    public DateTime? PayRateEffectiveDate { get; set; }

    /// <summary>
    /// Current vacation hours balance.
    /// </summary>
    public short VacationHoursBalance { get; set; }

    /// <summary>
    /// Current sick leave hours balance.
    /// </summary>
    public short SickLeaveHoursBalance { get; set; }

    /// <summary>
    /// Whether the employee is eligible for rehire.
    /// True if CurrentFlag = false and has at least one terminated assignment.
    /// </summary>
    public bool EligibleForRehire { get; set; }

    /// <summary>
    /// Number of times the employee has been rehired (count of terminated assignments).
    /// </summary>
    public int? RehireCount { get; set; }
}
