namespace AdventureWorks.Models.Features.Sales;

public sealed class CurrencyModel
{
    public string CurrencyCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
