using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class ScrapReason : BaseEntity, IAggregateRoot
    {

        public short ScrapReasonId { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<WorkOrder> WorkOrders { get; set; }
    }
}
