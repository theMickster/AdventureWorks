namespace AdventureWorks.Models.Features.Purchasing;

/// <summary>
/// Full detail for a single purchase order (<c>GET /api/v1/purchase-orders/{id}</c>).
/// </summary>
public sealed class PurchaseOrderDetailModel
{
    /// <summary>
    /// The purchase order's primary key.
    /// </summary>
    public int PurchaseOrderId { get; set; }

    /// <summary>
    /// The raw status code: 1=Pending, 2=Approved, 3=Rejected, 4=Complete.
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// The human-readable label for <see cref="Status"/>.
    /// </summary>
    public string StatusLabel { get; set; } = string.Empty;

    /// <summary>
    /// The date the purchase order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// The earliest (<c>MIN</c>) <c>DueDate</c> across the purchase order's line items.
    /// <c>PurchaseOrderHeader</c> has no <c>DueDate</c> column of its own. Falls back to
    /// <see cref="OrderDate"/> when the purchase order has zero line items.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// The date the purchase order shipped, or <c>null</c> if it has not shipped yet.
    /// </summary>
    public DateTime? ShipDate { get; set; }

    /// <summary>
    /// The vendor's primary key.
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// The approving employee's primary key.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// The approving employee's full name ("FirstName LastName"), resolved via
    /// <c>EmployeeEntity.PersonBusinessEntity</c>. Falls back to <c>"Unknown employee"</c> when
    /// the employee reference or its person join cannot be resolved — this is a functional
    /// requirement, not defensive-only null handling, since AdventureWorks data can contain
    /// unresolvable employee references.
    /// </summary>
    public string ApprovingEmployeeFullName { get; set; } = string.Empty;

    /// <summary>
    /// The shipping method's primary key.
    /// </summary>
    public int ShipMethodId { get; set; }

    /// <summary>
    /// The sum of all line item totals, before tax and freight.
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// The tax amount.
    /// </summary>
    public decimal TaxAmt { get; set; }

    /// <summary>
    /// The freight charge.
    /// </summary>
    public decimal Freight { get; set; }

    /// <summary>
    /// The total amount due (<c>SubTotal + TaxAmt + Freight</c>).
    /// </summary>
    public decimal TotalDue { get; set; }

    /// <summary>
    /// The purchase order's line items. Empty when the purchase order has no line items.
    /// </summary>
    public IReadOnlyList<PurchaseOrderLineItemModel> LineItems { get; set; } = Array.Empty<PurchaseOrderLineItemModel>();
}
