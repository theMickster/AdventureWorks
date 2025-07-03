namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model for updating an employee's address information.
/// Updates only the address fields (not the AddressType in BusinessEntityAddress).
/// </summary>
public sealed class EmployeeAddressUpdateModel
{
    /// <summary>
    /// Address identifier.
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// First line of the address (street address).
    /// </summary>
    public required string AddressLine1 { get; set; }

    /// <summary>
    /// Second line of the address (apartment, suite, etc.).
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// State or province identifier.
    /// </summary>
    public int StateProvinceId { get; set; }

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    public required string PostalCode { get; set; }
}
