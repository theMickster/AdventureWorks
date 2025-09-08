namespace AdventureWorks.Models.Features.HumanResources;

public sealed class PayBandSummaryModel
{
    public required string DepartmentGroup { get; set; }
    public decimal AverageRate { get; set; }
    public decimal MinRate { get; set; }
    public decimal MaxRate { get; set; }
}
