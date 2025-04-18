using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadStoreQueryHandler (
    IMapper mapper,
    IStoreRepository storeRepository,
    IBusinessEntityContactEntityRepository beceRepository)
        :IRequestHandler<ReadStoreQuery, StoreModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IBusinessEntityContactEntityRepository _beceRepository = beceRepository ?? throw new ArgumentNullException(nameof(beceRepository));

    public async Task<StoreModel> Handle(ReadStoreQuery request, CancellationToken cancellationToken)
    {
        var storeEntity = await _storeRepository.GetStoreByIdAsync(request.Id);
        if (storeEntity == null)
        {
            return null;
        }

        var storeContacts = await _beceRepository.GetContactsByIdAsync(request.Id);
        var contactModels = _mapper.Map<List<StoreContactModel>>(storeContacts);
        var storeModel = _mapper.Map<StoreModel>(storeEntity);
        storeModel.StoreContacts = contactModels;
        return storeModel;
    }
}
