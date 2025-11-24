namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Represents the full detail of a single sales order, including line items and addresses.
/// </summary>
public sealed class SalesOrderDetailModel
{
    /// <summary>The unique identifier for the sales order.</summary>
    public int SalesOrderId { get; set; }

    /// <summary>The sales order number (e.g., "SO43659").</summary>
    public required string SalesOrderNumber { get; set; }

    /// <summary>The order date.</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>The order due date.</summary>
    public DateTime DueDate { get; set; }

    /// <summary>The shipment date, if the order has shipped.</summary>
    public DateTime? ShipDate { get; set; }

    /// <summary>
    /// The order status code (1=In process, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled).
    /// </summary>
    public byte Status { get; set; }

    /// <summary>The human-readable status description.</summary>
    public string StatusDescription { get; set; } = string.Empty;

    /// <summary>The order subtotal before tax and freight.</summary>
    public decimal SubTotal { get; set; }

    /// <summary>The tax amount.</summary>
    public decimal TaxAmt { get; set; }

    /// <summary>The freight amount.</summary>
    public decimal Freight { get; set; }

    /// <summary>The total amount due (SubTotal + TaxAmt + Freight).</summary>
    public decimal TotalDue { get; set; }

    /// <summary>The purchase order number provided by the customer, if any.</summary>
    public string? PurchaseOrderNumber { get; set; }

    /// <summary>The sales person identifier, or null if unassigned.</summary>
    public int? SalesPersonId { get; set; }

    /// <summary>The sales person full name (FirstName LastName), or null if unassigned.</summary>
    public string? SalesPersonName { get; set; }

    /// <summary>
    /// The customer display name. For individual customers this is "FirstName LastName";
    /// for store customers this is the store name. Empty if neither is available.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>The name of the sales territory, or null if unassigned.</summary>
    public string? TerritoryName { get; set; }

    /// <summary>The bill-to address.</summary>
    public required SalesOrderAddressModel BillToAddress { get; set; }

    /// <summary>The ship-to address.</summary>
    public required SalesOrderAddressModel ShipToAddress { get; set; }

    /// <summary>The individual line items on this order.</summary>
    public List<SalesOrderLineItemModel> LineItems { get; set; } = [];
}
