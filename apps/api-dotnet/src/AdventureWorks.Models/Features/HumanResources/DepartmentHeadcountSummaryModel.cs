namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model representing the active employee headcount summary for a department, used in the all-departments summary endpoint.
/// </summary>
public sealed class DepartmentHeadcountSummaryModel
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
    /// Organizational group this department belongs to.
    /// </summary>
    public required string GroupName { get; set; }

    /// <summary>
    /// Count of currently active employees assigned to this department.
    /// </summary>
    public int ActiveEmployeeCount { get; set; }
}
