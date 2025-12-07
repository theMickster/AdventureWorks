namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Read model representing a single row in the Customer LTV list.
/// </summary>
public sealed class CustomerListItemModel
{
    /// <summary>The customer's unique identifier.</summary>
    public int CustomerId { get; set; }

    /// <summary>Store name (store customers) or contact person's full name (individual customers).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Either <c>"Store"</c> or <c>"Individual"</c>.</summary>
    public string CustomerType { get; set; } = string.Empty;

    /// <summary>The customer's store id, or <c>null</c> for an individual customer.</summary>
    public int? StoreId { get; set; }

    /// <summary>
    /// Sequential rank (1, 2, 3, ...) by lifetime spend descending, tie-broken by CustomerId
    /// ascending. Stable regardless of search filtering.
    /// </summary>
    public int LtvRank { get; set; }

    /// <summary>Sum of every order the customer has placed. Zero when the customer has no orders.</summary>
    public decimal TotalSpend { get; set; }

    /// <summary>Total number of orders placed by this customer.</summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// <c>true</c> when the customer has never ordered, or their most recent order predates the
    /// 12-month cutoff measured from the most recent order date across all customers.
    /// </summary>
    public bool IsInactive { get; set; }
}
