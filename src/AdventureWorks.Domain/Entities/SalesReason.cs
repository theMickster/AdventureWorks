using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities
{
    public class SalesReason : BaseEntity
    {

        public int SalesReasonId { get; set; }
        public string Name { get; set; }
        public string ReasonType { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReason { get; set; }
    }
}
