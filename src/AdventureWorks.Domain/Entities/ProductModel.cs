using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities 
{
    public class ProductModel : BaseEntity, IAggregateRoot
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
}
