using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving all addresses for a store.
/// </summary>
public sealed class ReadStoreAddressListQueryHandler(
    IMapper mapper,
    IBusinessEntityAddressRepository businessEntityAddressRepository)
        : IRequestHandler<ReadStoreAddressListQuery, List<StoreAddressModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IBusinessEntityAddressRepository _businessEntityAddressRepository = businessEntityAddressRepository ?? throw new ArgumentNullException(nameof(businessEntityAddressRepository));

    /// <summary>
    /// Retrieves the address list for the specified store and maps it to the response model.
    /// </summary>
    /// <param name="request">The query containing the store ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task<List<StoreAddressModel>> Handle(ReadStoreAddressListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var addresses = await _businessEntityAddressRepository.GetAddressesByStoreIdAsync(request.StoreId, cancellationToken);

        if (addresses is null or { Count: 0 })
        {
            return [];
        }

        return _mapper.Map<List<StoreAddressModel>>(addresses);
    }
}
