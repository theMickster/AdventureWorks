namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Full detail for a single customer (<c>GET /api/v1/customers/{id}</c>), including LTV rank
/// and lifetime-spend metrics.
/// </summary>
public sealed class CustomerDetailModel
{
    /// <summary>The customer's unique identifier.</summary>
    public int CustomerId { get; set; }

    /// <summary>Store name (store customers) or contact person's full name (individual customers).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Sequential rank (1, 2, 3, ...) by lifetime spend descending, tie-broken by CustomerId
    /// ascending. Computed via the same full-table ranking method as <c>GET /v1/customers</c>,
    /// so a customer's rank is identical between the list and detail endpoints.
    /// </summary>
    public int LtvRank { get; set; }

    /// <summary>Total number of customers ranked — the denominator for <see cref="LtvRank"/>.</summary>
    public int TotalCustomerCount { get; set; }

    /// <summary>Sum of every order the customer has placed. Zero when the customer has no orders.</summary>
    public decimal TotalSpend { get; set; }

    /// <summary>Total number of orders placed by this customer.</summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// <see cref="TotalSpend"/> divided by <see cref="OrderCount"/>. <c>0</c> when the customer
    /// has no orders, rather than dividing by zero.
    /// </summary>
    public decimal AvgOrderValue { get; set; }

    /// <summary>
    /// Date of the customer's most recent order, or <c>null</c> when the customer has no orders.
    /// </summary>
    public DateTime? LastOrderDate { get; set; }

    /// <summary>
    /// <c>true</c> when the customer has never ordered, or their most recent order predates the
    /// 12-month cutoff measured from the most recent order date across all customers.
    /// </summary>
    public bool IsInactive { get; set; }

    /// <summary>Either <c>"Store"</c> or <c>"Individual"</c>.</summary>
    public string CustomerType { get; set; } = string.Empty;

    /// <summary>The customer's store id, or <c>null</c> for an individual customer.</summary>
    public int? StoreId { get; set; }

    /// <summary>The customer's store name, or <c>null</c> for an individual customer.</summary>
    public string? StoreName { get; set; }

    /// <summary>The customer's first name, or <c>null</c> for a store customer.</summary>
    public string? FirstName { get; set; }

    /// <summary>The customer's last name, or <c>null</c> for a store customer.</summary>
    public string? LastName { get; set; }
}
