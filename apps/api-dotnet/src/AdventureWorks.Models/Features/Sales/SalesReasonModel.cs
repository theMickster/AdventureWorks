namespace AdventureWorks.Models.Features.Sales;

public sealed class SalesReasonModel
{
    public int SalesReasonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ReasonType { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
