using AdventureWorks.Common.Filtering.Base;

namespace AdventureWorks.Common.Filtering;

/// <summary>
/// Model for searching employees by various criteria.
/// </summary>
public sealed class EmployeeSearchModel : SearchPersonModelBase
{
    /// <summary>
    /// The employee's job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// The employee's email address.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// The employee's national ID number.
    /// </summary>
    public string? NationalIdNumber { get; set; }

    /// <summary>
    /// The employee's login ID.
    /// </summary>
    public string? LoginId { get; set; }
}
