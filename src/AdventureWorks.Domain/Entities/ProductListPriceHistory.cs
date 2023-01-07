using System;

namespace AdventureWorks.Domain.Entities;

public class ProductListPriceHistory : BaseEntity
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal ListPrice { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Product Product { get; set; }
}