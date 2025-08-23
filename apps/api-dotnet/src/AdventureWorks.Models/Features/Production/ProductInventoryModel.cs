namespace AdventureWorks.Models.Features.Production;

public sealed class ProductInventoryModel
{
    public short LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string Shelf { get; set; } = string.Empty;

    public byte Bin { get; set; }

    public short Quantity { get; set; }
}
