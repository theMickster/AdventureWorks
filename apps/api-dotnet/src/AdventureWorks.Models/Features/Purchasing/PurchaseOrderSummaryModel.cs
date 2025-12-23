namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// A single row in a vendor's purchase order history (<c>GET /api/v1/vendors/{id}/purchase-orders</c>).
/// </summary>
public sealed class PurchaseOrderSummaryModel
{
    /// <summary>
    /// The purchase order's primary key.
    /// </summary>
    public int PurchaseOrderId { get; set; }

    /// <summary>
    /// The date the purchase order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// The earliest (<c>MIN</c>) <c>DueDate</c> across the purchase order's line items.
    /// <c>PurchaseOrderHeader</c> has no <c>DueDate</c> column of its own — a PO-level due date is
    /// a derived value, and the earliest line-item due date is the one that actually drives when
    /// the vendor is expected to have fulfilled something. Null only when the header has no line
    /// items (a data-integrity edge case, not expected in practice).
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// The raw status code: 1=Pending, 2=Approved, 3=Rejected, 4=Complete.
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// The human-readable label for <see cref="Status"/>.
    /// </summary>
    public string StatusLabel { get; set; } = string.Empty;

    /// <summary>
    /// The purchase order's total amount due.
    /// </summary>
    public decimal TotalDue { get; set; }
}
