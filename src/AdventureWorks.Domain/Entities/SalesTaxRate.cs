using System;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class SalesTaxRate : BaseEntity, IAggregateRoot
    {
        public int SalesTaxRateId { get; set; }
        public int StateProvinceId { get; set; }
        public byte TaxType { get; set; }
        public decimal TaxRate { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual StateProvince StateProvince { get; set; }
    }
}
