namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Represents a single line item on a sales order.
/// </summary>
public sealed class SalesOrderLineItemModel
{
    /// <summary>The line item identifier.</summary>
    public int SalesOrderDetailId { get; set; }

    /// <summary>The product name.</summary>
    public required string ProductName { get; set; }

    /// <summary>The quantity ordered.</summary>
    public short OrderQty { get; set; }

    /// <summary>The unit price at time of sale.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>The computed line total (Qty * UnitPrice * (1 - UnitPriceDiscount)).</summary>
    public decimal LineTotal { get; set; }
}
