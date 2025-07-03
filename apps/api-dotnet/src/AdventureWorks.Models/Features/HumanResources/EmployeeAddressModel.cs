using AdventureWorks.Models.Slim;

namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model representing an employee's address with address type information.
/// </summary>
public sealed class EmployeeAddressModel
{
    /// <summary>
    /// Address identifier.
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// First line of the address (street address).
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Second line of the address (apartment, suite, etc.).
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State or province information (ID and Name).
    /// </summary>
    public GenericSlimModel? StateProvince { get; set; }

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Address type information (ID and Name) - e.g., Home, Work, Billing.
    /// </summary>
    public GenericSlimModel? AddressType { get; set; }

    /// <summary>
    /// Last modification date of the address.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
