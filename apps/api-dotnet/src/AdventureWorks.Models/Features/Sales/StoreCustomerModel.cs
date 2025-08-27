namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Read model representing a customer associated with a store, enriched with
/// lightweight order aggregates (lifetime spend, order count, last order date).
/// </summary>
public sealed class StoreCustomerModel
{
    /// <summary>The customer's unique identifier.</summary>
    public int CustomerId { get; set; }

    /// <summary>The customer's account number.</summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// The contact person's full name (FirstName + " " + LastName) or an empty string
    /// when the customer has no person record (store-only customer with no contact).
    /// </summary>
    public string PersonName { get; set; } = string.Empty;

    /// <summary>
    /// Sum of <c>SalesOrderHeader.TotalDue</c> across every order placed by this customer.
    /// Zero when the customer has no orders.
    /// </summary>
    public decimal LifetimeSpend { get; set; }

    /// <summary>Total number of orders placed by this customer.</summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Date of the customer's most recent order, or <c>null</c> when the customer has no orders.
    /// </summary>
    public DateTime? LastOrderDate { get; set; }
}
