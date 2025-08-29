namespace AdventureWorks.Models.Features.Sales;

public sealed class SpecialOfferModel
{
    public int SpecialOfferId { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal DiscountPct { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MinQty { get; set; }

    public int? MaxQty { get; set; }

    public bool IsActive { get; set; }

    public DateTime ModifiedDate { get; set; }
}
