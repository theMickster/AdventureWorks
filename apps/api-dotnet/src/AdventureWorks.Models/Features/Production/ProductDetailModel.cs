namespace AdventureWorks.Models.Features.Production;

public sealed class ProductDetailModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProductNumber { get; set; } = string.Empty;

    public bool MakeFlag { get; set; }

    public bool FinishedGoodsFlag { get; set; }

    public string? Color { get; set; }

    public short SafetyStockLevel { get; set; }

    public short ReorderPoint { get; set; }

    public decimal StandardCost { get; set; }

    public decimal ListPrice { get; set; }

    public string? Size { get; set; }

    public string? SizeUnitMeasureCode { get; set; }

    public string? WeightUnitMeasureCode { get; set; }

    public decimal? Weight { get; set; }

    public int DaysToManufacture { get; set; }

    public string? ProductLine { get; set; }

    public string? Class { get; set; }

    public string? Style { get; set; }

    public DateTime SellStartDate { get; set; }

    public DateTime? SellEndDate { get; set; }

    public DateTime? DiscontinuedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public ProductCategoryModel? Category { get; set; }

    public ProductSubcategoryModel? Subcategory { get; set; }

    public ProductModelInfoModel? Model { get; set; }

    public List<ProductPhotoMetadataModel> Photos { get; set; } = [];

    public int TotalInventoryQuantity { get; set; }
}
