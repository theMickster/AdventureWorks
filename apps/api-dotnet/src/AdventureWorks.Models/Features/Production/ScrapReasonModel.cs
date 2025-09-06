namespace AdventureWorks.Models.Features.Production;

public sealed class ScrapReasonModel
{
    public short ScrapReasonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
