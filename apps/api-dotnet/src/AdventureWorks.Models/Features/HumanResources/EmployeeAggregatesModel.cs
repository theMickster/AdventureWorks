namespace AdventureWorks.Models.Features.HumanResources;

public sealed class EmployeeAggregatesModel
{
    public required IReadOnlyList<DepartmentHeadcountSummaryModel> DepartmentHeadcounts { get; set; }
    public required TenureDistributionModel TenureDistribution { get; set; }
    public required IReadOnlyList<PayBandSummaryModel> PayBandSummary { get; set; }
}
