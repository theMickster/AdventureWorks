namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Search model for filtering sales orders.
/// </summary>
public sealed class SalesOrderSearchModel
{
    /// <summary>
    /// The filter by order date range start (inclusive).
    /// </summary>
    public DateTime? OrderDateFrom { get; set; }

    /// <summary>
    /// The filter by order date range end (inclusive).
    /// </summary>
    public DateTime? OrderDateTo { get; set; }

    /// <summary>
    /// The sales order status (1=In process, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled).
    /// </summary>
    public byte? Status { get; set; }

    /// <summary>
    /// The unique integer identifier of the sales person.
    /// </summary>
    public int? SalesPersonId { get; set; }

    /// <summary>
    /// The unique integer identifier of the sales territory.
    /// </summary>
    public int? TerritoryId { get; set; }

    /// <summary>
    /// The customer account number to filter by (exact match, e.g. 10-4020-000676).
    /// </summary>
    public string? AccountNumber { get; set; }
}
