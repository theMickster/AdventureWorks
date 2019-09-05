using System;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class EmployeePayHistory : BaseEntity, IAggregateRoot
    {
        public int BusinessEntityId { get; set; }
        public DateTime RateChangeDate { get; set; }
        public decimal Rate { get; set; }
        public byte PayFrequency { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual Employee BusinessEntity { get; set; }
    }
}
