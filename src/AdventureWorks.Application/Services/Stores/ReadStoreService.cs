using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Models.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Services.Stores;

[ServiceLifetimeScoped]
public sealed class ReadStoreService : IReadStoreService
{
    private readonly IMapper _mapper;
    private readonly IStoreRepository _storeRepository;
    private readonly IBusinessEntityContactEntityRepository _businessEntityContactEntityRepository;

    public ReadStoreService(
        IMapper mapper,
        IStoreRepository storeRepository,
        IBusinessEntityContactEntityRepository businessEntityContactEntityRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _businessEntityContactEntityRepository = businessEntityContactEntityRepository ?? throw new ArgumentNullException(nameof(businessEntityContactEntityRepository));
    }

    /// <summary>
    /// Retrieve a store using its unique identifier.
    /// </summary>
    /// <returns>A <see cref="StoreModel"/> </returns>
    public async Task<StoreModel?> GetByIdAsync(int storeId)
    {
        var storeEntity = await _storeRepository.GetStoreByIdAsync(storeId).ConfigureAwait(false);
        if (storeEntity == null)
        {
            return null;
        }

        var storeContacts = 
            await _businessEntityContactEntityRepository.GetContactsByIdAsync(storeId).ConfigureAwait(false);

        var contactModels = _mapper.Map<List<StoreContactModel>>(storeContacts);

        var storeModel = _mapper.Map<StoreModel>(storeEntity);

        storeModel.StoreContacts = contactModels;

        return storeModel;
    }

    /// <summary>
    /// Retrieves a paginated list of stores
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <returns>a <seealso cref="StoreSearchResultModel"/> object</returns>
    public async Task<StoreSearchResultModel> GetStoresAsync(StoreParameter parameters)
    {
        var result = new StoreSearchResultModel
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = 0
        };

        var (storeEntities, totalRecords) = await _storeRepository.GetStoresAsync(parameters).ConfigureAwait(false);

        if (storeEntities == null || !storeEntities.Any())
        {
            return result;
        }

        var stores = await CraftStoreModels(storeEntities).ConfigureAwait(false);

        result.Results = stores;
        result.TotalRecords = totalRecords;

        return result;
    }

    /// <summary>
    /// Retrieves a paged list of stores that is filtered using the <paramref name="storeSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="storeSearchModel">the input search parameters</param>
    /// <returns>a <seealso cref="StoreSearchResultModel"/> object</returns>
    public async Task<StoreSearchResultModel> SearchStoresAsync(
        StoreParameter parameters,
        StoreSearchModel storeSearchModel)
    {
        var result = new StoreSearchResultModel
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = 0
        };

        var (storeEntities, totalRecords) = await _storeRepository.SearchStoresAsync(parameters, storeSearchModel).ConfigureAwait(false);

        if (storeEntities == null || !storeEntities.Any())
        {
            return result;
        }

        var stores = await CraftStoreModels(storeEntities).ConfigureAwait(false);

        result.Results = stores;
        result.TotalRecords = totalRecords;

        return new StoreSearchResultModel();
    }


    #region Private Methods

    /// <summary>
    /// Create store models from store entities and store contact entities
    /// </summary>
    /// <param name="storeEntities">the list of store entities from the data access layer</param>
    /// <returns></returns>
    private async Task<List<StoreModel>> CraftStoreModels(IReadOnlyList<StoreEntity> storeEntities)
    {
        var contactModels = _mapper.Map<List<StoreContactModel>>(await _businessEntityContactEntityRepository
            .GetContactsByStoreIdsAsync(storeEntities.Select(x => x.BusinessEntityId).ToList()).ConfigureAwait(false));

        var stores = _mapper.Map<List<StoreModel>>(storeEntities);

        stores.ForEach(y => { y.StoreContacts = contactModels.Where(x => x.StoreId == y.Id).ToList(); });

        return stores;
    }
    
    #endregion Private Methods

}
