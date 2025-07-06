namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Input model for hiring an employee (onboarding workflow).
/// Orchestrates department assignment, pay rate setup, manager assignment, and PTO grants.
/// </summary>
public sealed class EmployeeHireModel
{
    /// <summary>
    /// Employee's BusinessEntityId (must already exist from CreateEmployee).
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Effective hire date. Cannot be more than 30 days in the future.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Initial department assignment (references HumanResources.Department).
    /// </summary>
    public short DepartmentId { get; set; }

    /// <summary>
    /// Initial shift assignment: 1=Day, 2=Evening, 3=Night (references HumanResources.Shift).
    /// </summary>
    public byte ShiftId { get; set; }

    /// <summary>
    /// Initial pay rate (hourly or annual depending on SalariedFlag).
    /// Must be greater than 0 and less than or equal to $500.00.
    /// </summary>
    public decimal InitialPayRate { get; set; }

    /// <summary>
    /// Pay frequency: 1=Monthly, 2=Bi-weekly.
    /// </summary>
    public byte PayFrequency { get; set; }

    /// <summary>
    /// Manager's BusinessEntityId for reporting relationship (optional).
    /// Used for OrganizationNode hierarchy updates.
    /// </summary>
    public int? ManagerId { get; set; }

    /// <summary>
    /// Initial vacation hours grant. Default: 40 hours for new hires.
    /// Must be between 0 and 240 hours.
    /// </summary>
    public short InitialVacationHours { get; set; } = 40;

    /// <summary>
    /// Initial sick leave hours grant. Default: 24 hours for new hires.
    /// Must be between 0 and 480 hours.
    /// </summary>
    public short InitialSickLeaveHours { get; set; } = 24;

    /// <summary>
    /// Optional hire notes or comments (e.g., "Transferred from partner company", "Rehire with seniority").
    /// Maximum 500 characters.
    /// </summary>
    public string? Notes { get; set; }
}
