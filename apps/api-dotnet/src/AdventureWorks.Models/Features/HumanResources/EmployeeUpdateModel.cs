namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model for updating an existing employee's personal and marital information.
/// Note: JobTitle, PTO, and compensation changes use separate endpoints.
/// Immutable fields (NationalIdNumber, LoginId, BirthDate, HireDate) cannot be updated.
/// </summary>
public sealed class EmployeeUpdateModel
{
    /// <summary>
    /// Employee's business entity ID (primary key).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Employee's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Employee's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Employee's middle name.
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

    /// <summary>
    /// Employee's marital status. Valid values: 'M' (Married), 'S' (Single).
    /// </summary>
    public required string MaritalStatus { get; set; }

    /// <summary>
    /// Employee's gender. Valid values: 'M' (Male), 'F' (Female).
    /// </summary>
    public required string Gender { get; set; }
}
