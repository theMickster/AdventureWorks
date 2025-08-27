using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="ReadStoreCustomerListQuery"/>.
/// Verifies the store exists, then loads the requested page of customer projections
/// (with per-customer order aggregates) via a narrow EF projection. Returns <c>null</c>
/// when the store does not exist; returns a populated result with empty
/// <see cref="StoreCustomerSearchResultModel.Results"/> and zero <see cref="StoreCustomerSearchResultModel.TotalRecords"/>
/// when the store exists but has no customers.
/// </summary>
public sealed class ReadStoreCustomerListQueryHandler(IMapper mapper, IStoreRepository storeRepository)
    : IRequestHandler<ReadStoreCustomerListQuery, StoreCustomerSearchResultModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));

    public async Task<StoreCustomerSearchResultModel?> Handle(ReadStoreCustomerListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var exists = await _storeRepository.ExistsAsync(request.StoreId, cancellationToken);
        if (!exists)
        {
            return null;
        }

        var (projections, totalCount) = await _storeRepository.GetCustomersByStoreIdAsync(
            request.StoreId, request.Parameters, cancellationToken);

        return new StoreCustomerSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = totalCount,
            Results = _mapper.Map<List<StoreCustomerModel>>(projections)
        };
    }
}
