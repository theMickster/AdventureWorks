using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Handles <see cref="ReadPurchasingAnalyticsQuery"/>; delegates directly to the purchasing
/// analytics repository which executes the aggregate queries and assembles the response model.
/// </summary>
public sealed class ReadPurchasingAnalyticsQueryHandler(IPurchasingAnalyticsRepository purchasingAnalyticsRepository)
    : IRequestHandler<ReadPurchasingAnalyticsQuery, PurchasingAnalyticsModel>
{
    private readonly IPurchasingAnalyticsRepository _purchasingAnalyticsRepository = purchasingAnalyticsRepository
        ?? throw new ArgumentNullException(nameof(purchasingAnalyticsRepository));

    /// <summary>
    /// Retrieves the purchasing analytics aggregate data.
    /// </summary>
    /// <param name="request">the query (no parameters required)</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>a <see cref="PurchasingAnalyticsModel"/> containing the Pareto vendor-spend data and the pipeline summary</returns>
    public Task<PurchasingAnalyticsModel> Handle(ReadPurchasingAnalyticsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _purchasingAnalyticsRepository.GetPurchasingAnalyticsAsync(cancellationToken);
    }
}
