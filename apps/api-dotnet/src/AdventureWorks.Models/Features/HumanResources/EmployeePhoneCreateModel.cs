namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model for creating an employee's phone number during employee creation.
/// Maps to Person.PersonPhone table.
/// </summary>
public sealed class EmployeePhoneCreateModel
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
