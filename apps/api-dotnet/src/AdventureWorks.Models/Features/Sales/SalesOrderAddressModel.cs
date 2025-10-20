namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Represents a bill-to or ship-to address on a sales order.
/// </summary>
public sealed class SalesOrderAddressModel
{
    /// <summary>The first address line.</summary>
    public required string AddressLine1 { get; set; }

    /// <summary>The second address line, if present.</summary>
    public string? AddressLine2 { get; set; }

    /// <summary>The city.</summary>
    public required string City { get; set; }

    /// <summary>The state or province name.</summary>
    public required string StateProvince { get; set; }

    /// <summary>The postal code.</summary>
    public required string PostalCode { get; set; }
}
