using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Production;

/// <summary>
/// Repository contract for the manufacturing quality scorecard.
/// </summary>
public interface IManufacturingQualityScorecardRepository
{
    /// <summary>
    /// Retrieves product quality rankings and scrap-reason aggregates across production work orders.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The manufacturing quality scorecard.</returns>
    Task<ManufacturingQualityScorecardModel> GetQualityScorecardAsync(
        CancellationToken cancellationToken = default);
}
