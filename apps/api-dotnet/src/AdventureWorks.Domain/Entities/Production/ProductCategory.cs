namespace AdventureWorks.Domain.Entities.Production;

public class ProductCategory : BaseEntity
{

    public int ProductCategoryId { get; set; }
    public string Name { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<ProductSubcategory> ProductSubcategories { get; set; }
}