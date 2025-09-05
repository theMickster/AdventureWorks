namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model representing the active employee headcount for a single department.
/// </summary>
public sealed class DepartmentHeadcountModel
{
    /// <summary>
    /// Unique department identifier.
    /// </summary>
    public short DepartmentId { get; set; }

    /// <summary>
    /// Department name.
    /// </summary>
    public required string DepartmentName { get; set; }

    /// <summary>
    /// Count of currently active employees assigned to this department.
    /// </summary>
    public int ActiveEmployeeCount { get; set; }
}
