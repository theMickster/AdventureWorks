using System;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class SalesTerritoryHistory : BaseEntity, IAggregateRoot
    {
        public int BusinessEntityId { get; set; }
        public int TerritoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual SalesPerson BusinessEntity { get; set; }
        public virtual SalesTerritory Territory { get; set; }
    }
}
