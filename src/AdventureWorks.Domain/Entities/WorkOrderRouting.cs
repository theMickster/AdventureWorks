﻿namespace AdventureWorks.Domain.Entities;

public class WorkOrderRouting : BaseEntity
{
    public int WorkOrderId { get; set; }
    public int ProductId { get; set; }
    public short OperationSequence { get; set; }
    public short LocationId { get; set; }
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal? ActualResourceHrs { get; set; }
    public decimal PlannedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Location Location { get; set; }
    public virtual WorkOrder WorkOrder { get; set; }
    public virtual Product Product { get; set; }
}