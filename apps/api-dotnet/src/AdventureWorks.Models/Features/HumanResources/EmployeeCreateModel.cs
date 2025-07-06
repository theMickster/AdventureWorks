using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model for creating a new employee in the HumanResources system.
/// Includes person data, employee-specific data, and optional contact information.
/// </summary>
public sealed class EmployeeCreateModel : EmployeeBaseModel
{
    /// <summary>
    /// Unique national identification number (e.g., SSN in US).
    /// </summary>
    public required string NationalIdNumber { get; set; }

    /// <summary>
    /// Network login identification.
    /// </summary>
    public required string LoginId { get; set; }

    /// <summary>
    /// Employee's date of birth. Must be at least 18 years in the past.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Phone number information.
    /// Creates an entry in Person.PersonPhone.
    /// </summary>
    /// <remarks>All employees must have at least one contact phone number.</remarks>
    public required EmployeePhoneCreateModel Phone { get; set; }

    /// <summary>
    /// Email address for business communications.
    /// Creates an entry in Person.EmailAddress.
    /// </summary>
    /// <remarks>Email address is mandatory for all employee records.</remarks>
    public required string EmailAddress { get; set; }

    /// <summary>
    /// Physical address information.
    /// Creates entries in Person.Address and Person.BusinessEntityAddress.
    /// </summary>
    /// <remarks>Every employee must have a registered address on file.</remarks>
    public required AddressCreateModel Address { get; set; }

    /// <summary>
    /// Type of address (Home, Work, etc.).
    /// References Person.AddressType.AddressTypeID.
    /// </summary>
    /// <remarks>Address type must be specified when providing an address.</remarks>
    public int AddressTypeId { get; set; }
}
