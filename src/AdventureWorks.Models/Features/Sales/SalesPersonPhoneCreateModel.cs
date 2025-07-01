namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Model for creating a sales person's phone number during sales person creation.
/// Maps to Person.PersonPhone table.
/// </summary>
public sealed class SalesPersonPhoneCreateModel
{
    /// <summary>
    /// Phone number in the format specified by the organization.
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Type of phone number (Cell, Home, Work, etc.).
    /// References Person.PhoneNumberType.PhoneNumberTypeID.
    /// </summary>
    public int PhoneNumberTypeId { get; set; }
}
