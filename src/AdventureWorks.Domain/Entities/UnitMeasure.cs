using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class UnitMeasure : BaseEntity, IAggregateRoot
    {
        public string UnitMeasureCode { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<BillOfMaterials> BillOfMaterials { get; set; }
        public ICollection<Product> ProductSizeUnitMeasureCodeNavigation { get; set; }
        public ICollection<Product> ProductWeightUnitMeasureCodeNavigation { get; set; }
        public ICollection<ProductVendor> ProductVendors { get; set; }
    }
}
