using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

/// <summary>
/// Repository interface for sales dashboard aggregate data operations.
/// </summary>
public interface ISalesDashboardRepository
{
    /// <summary>
    /// Retrieves the aggregate KPI data for the sales dashboard, including overall metrics,
    /// top performers, territory breakdown, and monthly sales trend for the trailing 24 months.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A <see cref="SalesDashboardModel"/> containing all dashboard aggregate data</returns>
    Task<SalesDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default);
}
