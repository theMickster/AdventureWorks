using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving all contacts for a store.
/// </summary>
public sealed class ReadStoreContactListQueryHandler(
    IMapper mapper,
    IBusinessEntityContactEntityRepository beceRepository)
        : IRequestHandler<ReadStoreContactListQuery, List<StoreContactModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IBusinessEntityContactEntityRepository _beceRepository = beceRepository ?? throw new ArgumentNullException(nameof(beceRepository));

    /// <summary>
    /// Retrieves the contact list for the specified store and maps it to the response model.
    /// </summary>
    /// <param name="request">The query containing the store ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task<List<StoreContactModel>> Handle(ReadStoreContactListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var contacts = await _beceRepository.GetContactsByIdAsync(request.StoreId);

        if (contacts is null or { Count: 0 })
        {
            return new List<StoreContactModel>();
        }

        return _mapper.Map<List<StoreContactModel>>(contacts);
    }
}
