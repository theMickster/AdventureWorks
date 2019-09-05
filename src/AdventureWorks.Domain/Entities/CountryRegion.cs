using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class CountryRegion : BaseEntity, IAggregateRoot
    {

        public string CountryRegionCode { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<CountryRegionCurrency> CountryRegionCurrency { get; set; }
        public virtual ICollection<SalesTerritory> SalesTerritory { get; set; }
        public virtual ICollection<StateProvince> StateProvince { get; set; }
    }
}
