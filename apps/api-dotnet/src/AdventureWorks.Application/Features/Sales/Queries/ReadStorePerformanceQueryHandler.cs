using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="ReadStorePerformanceQuery"/>.
/// Loads the store's YTD aggregates via a narrow projection, then computes the average order
/// value post-map. Returns <c>null</c> when the store does not exist; returns a model with zero
/// aggregate values when the store has no customers/orders for the year.
/// </summary>
public sealed class ReadStorePerformanceQueryHandler(IMapper mapper, IStoreRepository storeRepository)
    : IRequestHandler<ReadStorePerformanceQuery, StorePerformanceModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));

    public async Task<StorePerformanceModel?> Handle(ReadStorePerformanceQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Single source of truth for what year YTD covers — the handler decides, callers cannot override.
        var year = DateTime.UtcNow.Year;

        var projection = await _storeRepository.GetPerformanceAsync(request.StoreId, year, cancellationToken);
        if (projection is null)
        {
            return null;
        }

        var model = _mapper.Map<StorePerformanceModel>(projection);
        model.AverageOrderValue = model.OrderCount == 0 ? 0m : model.RevenueYtd / model.OrderCount;

        return model;
    }
}
