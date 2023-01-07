using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class ScrapReason : BaseEntity
{

    public short ScrapReasonId { get; set; }
    public string Name { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; }
}