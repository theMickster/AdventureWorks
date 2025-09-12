namespace AdventureWorks.Models.Features.Person;

/// <summary>
/// Search result model for a person in list view.
/// </summary>
public sealed class SearchPersonsModel
{
    /// <summary>
    /// The person's BusinessEntityId.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// The person's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The person's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The name of the person type (e.g., "Employee", "Customer", "Vendor").
    /// </summary>
    public string PersonTypeName { get; set; } = string.Empty;

    /// <summary>
    /// The person's primary email address, or null if none exists.
    /// </summary>
    public string? PrimaryEmail { get; set; }
}
