using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class SalesTerritory : BaseEntity, IAggregateRoot
    {
        public int TerritoryId { get; set; }
        public string Name { get; set; }
        public string CountryRegionCode { get; set; }
        public string Group { get; set; }
        public decimal SalesYtd { get; set; }
        public decimal SalesLastYear { get; set; }
        public decimal CostYtd { get; set; }
        public decimal CostLastYear { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<Customer> Customers { get; set; }
        public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
        public ICollection<SalesPerson> SalesTerritorySalesPersons { get; set; }
        public ICollection<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
        public ICollection<StateProvince> StateProvinces { get; set; }
        public CountryRegion CountryRegionCodeNavigation { get; set; }
    }
}
