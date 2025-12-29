namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// A single line item on a purchase order (part of <c>GET /api/v1/purchase-orders/{id}</c>).
/// </summary>
public sealed class PurchaseOrderLineItemModel
{
    /// <summary>
    /// The line item's primary key.
    /// </summary>
    public int PurchaseOrderDetailId { get; set; }

    /// <summary>
    /// The ordered product's primary key.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// The ordered product's name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// The date this line item is expected to be received.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// The quantity ordered.
    /// </summary>
    public short OrderQty { get; set; }

    /// <summary>
    /// The unit price at time of order.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// The extended line total (<c>OrderQty * UnitPrice</c>).
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// The quantity received so far.
    /// </summary>
    public decimal ReceivedQty { get; set; }

    /// <summary>
    /// The quantity rejected on receipt.
    /// </summary>
    public decimal RejectedQty { get; set; }

    /// <summary>
    /// The quantity accepted and stocked.
    /// </summary>
    public decimal StockedQty { get; set; }
}
