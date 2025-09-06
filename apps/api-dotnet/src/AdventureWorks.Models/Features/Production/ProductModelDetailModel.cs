namespace AdventureWorks.Models.Features.Production;

public sealed class ProductModelDetailModel
{
    public int ProductModelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CatalogDescription { get; set; }

    public DateTime ModifiedDate { get; set; }
}
