namespace AdventureWorks.Models.Features.Production;

public sealed class ProductSubcategoryModel
{
    public int SubcategoryId { get; set; }

    public int ProductCategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string Name { get; set; } = string.Empty;
}
