using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class Product : BaseEntity
{

    public int ProductId { get; set; }
    public string Name { get; set; }
    public string ProductNumber { get; set; }
    public bool MakeFlag { get; set; }
    public bool FinishedGoodsFlag { get; set; }
    public string Color { get; set; }
    public short SafetyStockLevel { get; set; }
    public short ReorderPoint { get; set; }
    public decimal StandardCost { get; set; }
    public decimal ListPrice { get; set; }
    public string Size { get; set; }
    public string SizeUnitMeasureCode { get; set; }
    public string WeightUnitMeasureCode { get; set; }
    public decimal? Weight { get; set; }
    public int DaysToManufacture { get; set; }
    public string ProductLine { get; set; }
    public string Class { get; set; }
    public string Style { get; set; }
    public int? ProductSubcategoryId { get; set; }
    public int? ProductModelId { get; set; }
    public DateTime SellStartDate { get; set; }
    public DateTime? SellEndDate { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<BillOfMaterials> BillOfMaterialsComponents { get; set; }
    public ICollection<BillOfMaterials> BillOfMaterialsProductAssemblies { get; set; }
    public ICollection<ProductCostHistory> ProductCostHistory { get; set; }
    public ICollection<ProductInventory> ProductInventory { get; set; }
    public ICollection<ProductListPriceHistory> ProductListPriceHistory { get; set; }
    public ICollection<ProductProductPhoto> ProductProductPhotos { get; set; }
    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<ProductVendor> ProductVendors { get; set; }
    public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    public ICollection<SpecialOfferProduct> SpecialOfferProducts { get; set; }
    public ICollection<TransactionHistory> TransactionHistory { get; set; }
    public ICollection<WorkOrderRouting> WorkOrderRoutings { get; set; }
    public ICollection<WorkOrder> WorkOrders { get; set; }

    public ProductModel ProductModel { get; set; }
    public ProductSubcategory ProductSubcategory { get; set; }
    public UnitMeasure SizeUnitMeasureCodeNavigation { get; set; }
    public UnitMeasure WeightUnitMeasureCodeNavigation { get; set; }
}