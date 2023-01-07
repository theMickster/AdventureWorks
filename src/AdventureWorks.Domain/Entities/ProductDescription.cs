using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class ProductDescription : BaseEntity
{
    public int ProductDescriptionId { get; set; }
    public string Description { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCulture { get; set; }
}