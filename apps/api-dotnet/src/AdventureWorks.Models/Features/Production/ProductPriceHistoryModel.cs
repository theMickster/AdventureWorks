namespace AdventureWorks.Models.Features.Production;

public sealed class ProductPriceHistoryModel
{
    public int ProductId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal Price { get; set; }

    /// <summary>
    /// Either "cost" or "list" indicating the type of price history record.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
