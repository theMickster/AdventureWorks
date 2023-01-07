using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class WorkOrder : BaseEntity
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