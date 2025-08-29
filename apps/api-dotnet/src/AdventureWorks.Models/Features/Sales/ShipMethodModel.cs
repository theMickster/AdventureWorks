namespace AdventureWorks.Models.Features.Sales;

public sealed class ShipMethodModel
{
    public int ShipMethodId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal ShipBase { get; set; }

    public decimal ShipRate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
