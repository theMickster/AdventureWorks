namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>Represents a single department assignment record in an employee's history.</summary>
public sealed class EmployeeDepartmentHistoryModel
{
    /// <summary>The department identifier.</summary>
    public short DepartmentId { get; set; }

    /// <summary>The department name.</summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>The shift identifier.</summary>
    public byte ShiftId { get; set; }

    /// <summary>The shift name.</summary>
    public string ShiftName { get; set; } = string.Empty;

    /// <summary>The date on which this assignment began.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>The date on which this assignment ended, or null if currently active.</summary>
    public DateTime? EndDate { get; set; }
}
