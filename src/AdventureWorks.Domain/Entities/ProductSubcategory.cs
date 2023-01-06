using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities
{
    public class ProductSubcategory : BaseEntity
    {
        public int ProductSubcategoryId { get; set; }
        public int ProductCategoryId { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<Product> Products { get; set; }
        public ProductCategory ProductCategory { get; set; }
    }
}
