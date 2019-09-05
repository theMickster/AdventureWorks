using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class WorkOrder : BaseEntity, IAggregateRoot
    {

        public int WorkOrderId { get; set; }
        public int ProductId { get; set; }
        public int OrderQty { get; set; }
        public int StockedQty { get; set; }
        public short ScrappedQty { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public short? ScrapReasonId { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<WorkOrderRouting> WorkOrderRoutings { get; set; }
        public Product Product { get; set; }
        public ScrapReason ScrapReason { get; set; }
    }
}
