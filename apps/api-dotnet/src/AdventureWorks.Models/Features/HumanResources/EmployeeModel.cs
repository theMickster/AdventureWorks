namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Model representing an employee for read operations.
/// </summary>
public sealed class EmployeeModel : EmployeeBaseModel
{
    /// <summary>
    /// Unique business entity identifier (primary key).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique national identification number (e.g., SSN in US).
    /// </summary>
    public required string NationalIdNumber { get; set; }

    /// <summary>
    /// Network login identification.
    /// </summary>
    public required string LoginId { get; set; }

    /// <summary>
    /// Employee's date of birth.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Employee's hire date.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Indicates whether the employee is currently employed.
    /// </summary>
    public bool CurrentFlag { get; set; }

    /// <summary>
    /// Number of available vacation hours.
    /// </summary>
    public short VacationHours { get; set; }

    /// <summary>
    /// Number of available sick leave hours.
    /// </summary>
    public short SickLeaveHours { get; set; }

    /// <summary>
    /// Email address for business communications.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Date when the employee record was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
