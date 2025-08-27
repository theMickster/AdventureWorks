using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="ReadStoreAddressQuery"/>.
/// Retrieves a single store address (with AddressType, Address, StateProvince, and CountryRegion details) by its composite key
/// and maps it to <see cref="StoreAddressModel"/>. Returns <c>null</c> when the address does not exist.
/// </summary>
public sealed class ReadStoreAddressQueryHandler(
    IMapper mapper,
    IBusinessEntityAddressRepository businessEntityAddressRepository)
        : IRequestHandler<ReadStoreAddressQuery, StoreAddressModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IBusinessEntityAddressRepository _businessEntityAddressRepository = businessEntityAddressRepository ?? throw new ArgumentNullException(nameof(businessEntityAddressRepository));

    public async Task<StoreAddressModel?> Handle(ReadStoreAddressQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _businessEntityAddressRepository.GetWithDetailsByCompositeKeyAsync(
            request.StoreId, request.AddressId, request.AddressTypeId, cancellationToken);

        return entity is null ? null : _mapper.Map<StoreAddressModel>(entity);
    }
}
