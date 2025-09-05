namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Represents a single pay history record for an employee.
/// </summary>
public sealed class EmployeePayHistoryModel
{
    /// <summary>
    /// Date the pay rate change became effective.
    /// </summary>
    public DateTime RateChangeDate { get; init; }

    /// <summary>
    /// Hourly pay rate.
    /// </summary>
    public decimal Rate { get; init; }

    /// <summary>
    /// Pay frequency code: 1 = Monthly, 2 = Bi-Weekly.
    /// </summary>
    public byte PayFrequency { get; init; }

    /// <summary>
    /// Human-readable pay frequency label derived from <see cref="PayFrequency"/>.
    /// </summary>
    public string PayFrequencyLabel { get; init; } = string.Empty;
}
