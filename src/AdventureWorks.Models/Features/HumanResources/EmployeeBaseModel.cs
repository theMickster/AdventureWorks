namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Base model containing common employee fields shared across Create and Update operations.
/// </summary>
public abstract class EmployeeBaseModel
{
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
    /// Employee's job title.
    /// </summary>
    public required string JobTitle { get; set; }

    /// <summary>
    /// Employee's marital status. Valid values: 'M' (Married), 'S' (Single).
    /// </summary>
    public required string MaritalStatus { get; set; }

    /// <summary>
    /// Employee's gender. Valid values: 'M' (Male), 'F' (Female).
    /// </summary>
    public required string Gender { get; set; }

    /// <summary>
    /// Indicates whether the employee is salaried (true) or hourly (false).
    /// </summary>
    public bool SalariedFlag { get; set; }

    /// <summary>
    /// Employee's level in the organization hierarchy.
    /// </summary>
    public short? OrganizationLevel { get; set; }
}
