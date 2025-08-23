using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving all addresses for a store.
/// </summary>
public sealed class ReadStoreAddressListQueryHandler(
    IMapper mapper,
    IStoreRepository storeRepository)
        : IRequestHandler<ReadStoreAddressListQuery, List<BusinessEntityAddressModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));

    /// <summary>
    /// Retrieves the address list for the specified store and maps it to the response model.
    /// </summary>
    /// <param name="request">The query containing the store ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task<List<BusinessEntityAddressModel>> Handle(ReadStoreAddressListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var addresses = await _storeRepository.GetAddressesByStoreIdAsync(request.StoreId, cancellationToken);

        if (addresses is null or { Count: 0 })
        {
            return [];
        }

        return _mapper.Map<List<BusinessEntityAddressModel>>(addresses);
    }
}
