using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handles <see cref="ReadSalesDashboardQuery"/>; delegates directly to the dashboard repository
/// which executes the aggregate queries and assembles the response model.
/// </summary>
public sealed class ReadSalesDashboardQueryHandler(ISalesDashboardRepository dashboardRepository)
    : IRequestHandler<ReadSalesDashboardQuery, SalesDashboardModel>
{
    private readonly ISalesDashboardRepository _dashboardRepository = dashboardRepository
        ?? throw new ArgumentNullException(nameof(dashboardRepository));

    /// <summary>
    /// Retrieves the sales dashboard aggregate data.
    /// </summary>
    /// <param name="request">the query (no parameters required)</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>a <see cref="SalesDashboardModel"/> containing all KPI aggregate data</returns>
    public Task<SalesDashboardModel> Handle(ReadSalesDashboardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _dashboardRepository.GetDashboardAsync(cancellationToken);
    }
}
