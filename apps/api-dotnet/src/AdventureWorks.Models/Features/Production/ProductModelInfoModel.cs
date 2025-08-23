namespace AdventureWorks.Models.Features.Production;

public sealed class ProductModelInfoModel
{
    public int ModelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CatalogDescription { get; set; }
}
