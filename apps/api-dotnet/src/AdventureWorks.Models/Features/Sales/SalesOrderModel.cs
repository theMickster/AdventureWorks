namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Represents a sales order in API responses.
/// </summary>
public sealed class SalesOrderModel
{
    /// <summary>
    /// The unique identifier for the sales order.
    /// </summary>
    public int SalesOrderId { get; set; }

    /// <summary>
    /// The sales order number (e.g., "SO43659").
    /// </summary>
    public required string SalesOrderNumber { get; set; }

    /// <summary>
    /// The order date.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// The order status (1=In process, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled).
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// The human-readable status description.
    /// </summary>
    public string StatusDescription { get; set; } = string.Empty;

    /// <summary>
    /// The total amount due (SubTotal + TaxAmt + Freight).
    /// </summary>
    public decimal TotalDue { get; set; }

    /// <summary>
    /// The customer display name. For individual customers this is "FirstName LastName";
    /// for store customers this is the store name. Empty if neither is available.
    /// </summary>
    public required string CustomerName { get; set; }

    /// <summary>
    /// The sales person name (FirstName LastName), or null if no sales person assigned.
    /// </summary>
    public string? SalesPersonName { get; set; }
}
