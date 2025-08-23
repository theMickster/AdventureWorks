namespace AdventureWorks.Models.Features.Production;

public sealed class ProductListModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProductNumber { get; set; } = string.Empty;

    public string? Color { get; set; }

    public decimal ListPrice { get; set; }

    public decimal StandardCost { get; set; }

    public string? CategoryName { get; set; }

    public string? SubcategoryName { get; set; }

    public string? ModelName { get; set; }

    public bool IsActive { get; set; }

    public DateTime ModifiedDate { get; set; }
}
