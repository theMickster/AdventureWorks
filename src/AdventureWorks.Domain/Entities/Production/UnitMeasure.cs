using AdventureWorks.Domain.Entities.Purchasing;

namespace AdventureWorks.Domain.Entities.Production;

public class UnitMeasure : BaseEntity
{
    public string UnitMeasureCode { get; set; }
    public string Name { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<BillOfMaterials> BillOfMaterials { get; set; }
    public ICollection<Product> ProductSizeUnitMeasureCodeNavigation { get; set; }
    public ICollection<Product> ProductWeightUnitMeasureCodeNavigation { get; set; }
    public ICollection<ProductVendor> ProductVendors { get; set; }
}