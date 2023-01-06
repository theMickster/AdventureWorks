using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities 
{
    public class SalesPerson : BaseEntity
    {
        public int BusinessEntityId { get; set; }
        public int? TerritoryId { get; set; }
        public decimal? SalesQuota { get; set; }
        public decimal Bonus { get; set; }
        public decimal CommissionPct { get; set; }
        public decimal SalesYtd { get; set; }
        public decimal SalesLastYear { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
        public ICollection<SalesPersonQuotaHistory> SalesPersonQuotaHistory { get; set; }
        public ICollection<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
        public ICollection<Store> Stores { get; set; }
        public Employee BusinessEntity { get; set; }
        public SalesTerritory Territory { get; set; }
    }
}
