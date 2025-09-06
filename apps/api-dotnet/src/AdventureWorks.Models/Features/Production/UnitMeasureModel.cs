namespace AdventureWorks.Models.Features.Production;

public sealed class UnitMeasureModel
{
    public string UnitMeasureCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
