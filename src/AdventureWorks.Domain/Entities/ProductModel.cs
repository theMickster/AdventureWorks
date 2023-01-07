using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class ProductModel : BaseEntity
{
    public int ProductModelId { get; set; }
    public string Name { get; set; }
    public string CatalogDescription { get; set; }
    public string Instructions { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<Product> Products { get; set; }
    public ICollection<ProductModelIllustration> ProductModelIllustration { get; set; }
    public ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
}