namespace AdventureWorks.Models.Features.HumanResources;

public sealed class EmployeeAggregatesModel
{
    /// <summary>
    /// Total number of employees in the database, active and terminated combined.
    /// </summary>
    public required int TotalEmployeeCount { get; set; }

    /// <summary>
    /// Number of employees with <c>CurrentFlag == true</c>.
    /// </summary>
    public required int ActiveEmployeeCount { get; set; }

    /// <summary>
    /// Number of employees with <c>CurrentFlag == false</c>.
    /// </summary>
    public required int TerminatedEmployeeCount { get; set; }

    /// <summary>
    /// Total number of departments, independent of whether they currently have active headcount.
    /// </summary>
    public required int DepartmentCount { get; set; }

    public required IReadOnlyList<DepartmentHeadcountSummaryModel> DepartmentHeadcounts { get; set; }

    public required TenureDistributionModel TenureDistribution { get; set; }

    public required IReadOnlyList<PayBandSummaryModel> PayBandSummary { get; set; }
}
