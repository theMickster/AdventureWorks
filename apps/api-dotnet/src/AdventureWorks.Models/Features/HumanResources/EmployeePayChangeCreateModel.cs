namespace AdventureWorks.Models.Features.HumanResources;

/// <summary>
/// Input model for recording a pay rate change for an employee.
/// </summary>
public sealed class EmployeePayChangeCreateModel
{
    /// <summary>
    /// New hourly pay rate. Must be between $6.50 and $200.00 (inclusive).
    /// </summary>
    public decimal Rate { get; init; }

    /// <summary>
    /// Pay frequency code: 1 = Monthly, 2 = Bi-Weekly.
    /// </summary>
    public byte PayFrequency { get; init; }
}
