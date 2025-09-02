using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving the full sales person assignment history for a store.
/// </summary>
public sealed class ReadStoreSalesPersonAssignmentListQueryHandler(
    IMapper mapper,
    IStoreRepository storeRepository,
    IStoreSalesPersonHistoryRepository historyRepository)
        : IRequestHandler<ReadStoreSalesPersonAssignmentListQuery, List<StoreSalesPersonAssignmentModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IStoreSalesPersonHistoryRepository _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));

    public async Task<List<StoreSalesPersonAssignmentModel>> Handle(ReadStoreSalesPersonAssignmentListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _storeRepository.ExistsAsync(request.StoreId, cancellationToken))
        {
            throw new KeyNotFoundException($"Store with ID {request.StoreId} was not found.");
        }

        var entities = await _historyRepository.GetAssignmentsByStoreIdAsync(request.StoreId, cancellationToken);

        if (entities is null or { Count: 0 })
        {
            return [];
        }

        return _mapper.Map<List<StoreSalesPersonAssignmentModel>>(entities);
    }
}
