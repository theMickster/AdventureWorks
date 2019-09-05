using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class SalesReason : BaseEntity, IAggregateRoot
    {

        public int SalesReasonId { get; set; }
        public string Name { get; set; }
        public string ReasonType { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReason { get; set; }
    }
}
