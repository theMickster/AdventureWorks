using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
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
    public async Task<StoreSearchResultModel> GetStores(StoreParameter parameters)
    {
        var (storeEntities, totalRecords) = await _storeRepository.GetStores(parameters).ConfigureAwait(false);
        var stores = _mapper.Map<List<StoreModel>>(storeEntities);

        var result = new StoreSearchResultModel
        {
            Results = stores,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = totalRecords
        };

        return result;
    }
}
