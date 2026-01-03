using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Production;

/// <summary>
/// Repository contract for aggregate manufacturing KPIs.
/// </summary>
public interface IManufacturingKpiRepository
{
    /// <summary>
    /// Retrieves manufacturing KPIs across every production work order.
    /// </summary>
    /// <remarks>
    /// The implementation aggregates the two stored quantity columns and derives stocked units
    /// as ordered units minus scrapped units. Yield and scrap percentages are calculated from the
    /// aggregate quantities, rounded to two decimal places, with zero returned for both percentages
    /// when no units were ordered.
    /// </remarks>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The aggregate manufacturing KPI model.</returns>
    Task<ManufacturingKpisModel> GetManufacturingKpisAsync(CancellationToken cancellationToken = default);
}
