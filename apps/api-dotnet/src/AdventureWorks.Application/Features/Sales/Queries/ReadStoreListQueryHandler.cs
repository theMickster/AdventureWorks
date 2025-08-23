using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;
namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadStoreListQueryHandler(
    IMapper mapper,
    IStoreRepository storeRepository,
    IBusinessEntityContactEntityRepository beceRepository)
        : IRequestHandler<ReadStoreListQuery, StoreSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IBusinessEntityContactEntityRepository _beceRepository = beceRepository ?? throw new ArgumentNullException(nameof(beceRepository));

    public async Task<StoreSearchResultModel> Handle(ReadStoreListQuery request, CancellationToken cancellationToken)
    {
        var result = new StoreSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0
        };

        IReadOnlyList<StoreEntity> storeEntities;
        var totalRecords = 0;

        if (request.SearchModel is null)
        {
            (storeEntities, totalRecords) = await _storeRepository.GetStoresAsync(request.Parameters, request.Parameters.IncludeAddresses, cancellationToken);
        }
        else
        {
            (storeEntities, totalRecords) = await _storeRepository.SearchStoresAsync(request.Parameters, request.SearchModel, request.Parameters.IncludeAddresses, cancellationToken);
        }

        if (storeEntities is null or { Count: 0 })
        {
            return result;
        }

        var stores = await CraftStoreModelsAsync(storeEntities, request.Parameters.IncludeContacts, cancellationToken);

        result.Results = stores;
        result.TotalRecords = totalRecords;

        return result;
    }

    #region Private Methods

    /// <summary>
    /// Create store models from store entities and store contact entities
    /// </summary>
    /// <param name="storeEntities">the list of store entities from the data access layer</param>
    /// <param name="includeContacts">whether to populate store contacts</param>
    /// <returns></returns>
    private async Task<List<StoreModel>> CraftStoreModelsAsync(
        IReadOnlyList<StoreEntity> storeEntities,
        bool includeContacts,
        CancellationToken cancellationToken)
    {
        var stores = _mapper.Map<List<StoreModel>>(storeEntities);

        if (includeContacts)
        {
            var contactModels = _mapper.Map<List<StoreContactModel>>(await _beceRepository
                .GetContactsByStoreIdsAsync(storeEntities.Select(x => x.BusinessEntityId).ToList(), cancellationToken).ConfigureAwait(false));

            stores.ForEach(y => { y.StoreContacts = contactModels.Where(x => x.StoreId == y.Id).ToList(); });
        }
        else
        {
            stores.ForEach(y => { y.StoreContacts = []; });
        }

        return stores;
    }

    #endregion Private Methods
}
