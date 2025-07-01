using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Model for creating a new sales person with complete Employee/Person entity graph.
/// Creates BusinessEntity → Person → Employee → SalesPerson in one transaction.
/// </summary>
public sealed class SalesPersonCreateModel : SalesPersonBaseModel
{
    // Person fields
    /// <summary>
    /// Sales person's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Sales person's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Sales person's middle name.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Title prefix (Mr., Ms., Dr., etc.).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Name suffix (Jr., Sr., III, etc.).
    /// </summary>
    public string? Suffix { get; set; }

    // Employee-specific fields
    /// <summary>
    /// Unique national identification number (e.g., SSN in US).
    /// </summary>
    public required string NationalIdNumber { get; set; }

    /// <summary>
    /// Network login identification.
    /// </summary>
    public required string LoginId { get; set; }

    /// <summary>
    /// Sales person's job title.
    /// </summary>
    public required string JobTitle { get; set; }

    /// <summary>
    /// Sales person's date of birth. Must be at least 18 years before hire date.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Sales person's hire date. Cannot be in the future.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Sales person's marital status. Valid values: 'M' (Married), 'S' (Single).
    /// </summary>
    public required string MaritalStatus { get; set; }

    /// <summary>
    /// Sales person's gender. Valid values: 'M' (Male), 'F' (Female).
    /// </summary>
    public required string Gender { get; set; }

    /// <summary>
    /// Indicates whether the sales person is salaried (true) or hourly (false).
    /// </summary>
    public bool SalariedFlag { get; set; }

    /// <summary>
    /// Sales person's level in the organization hierarchy.
    /// </summary>
    public short? OrganizationLevel { get; set; }

    // Contact fields
    /// <summary>
    /// Phone number information.
    /// Creates an entry in Person.PersonPhone.
    /// </summary>
    public required SalesPersonPhoneCreateModel Phone { get; set; }

    /// <summary>
    /// Email address for business communications.
    /// Creates an entry in Person.EmailAddress.
    /// </summary>
    public required string EmailAddress { get; set; }

    /// <summary>
    /// Physical address information.
    /// Creates entries in Person.Address and Person.BusinessEntityAddress.
    /// </summary>
    public required AddressCreateModel Address { get; set; }

    /// <summary>
    /// Type of address (Home, Work, etc.).
    /// References Person.AddressType.AddressTypeID.
    /// </summary>
    public int AddressTypeId { get; set; }
}
