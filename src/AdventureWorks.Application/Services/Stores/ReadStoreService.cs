using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Models.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Services.Stores;

[ServiceLifetimeScoped]
public sealed class ReadStoreService : IReadStoreService
{
    private readonly IMapper _mapper;
    private readonly IStoreRepository _storeRepository;

    public ReadStoreService(
        IMapper mapper,
        IStoreRepository storeRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    }

    /// <summary>
    /// Retrieve a store using its unique identifier.
    /// </summary>
    /// <returns>A <see cref="StoreModel"/> </returns>
    public async Task<StoreModel?> GetByIdAsync(int storeId)
    {
        var storeEntity = await _storeRepository.GetStoreByIdAsync(storeId).ConfigureAwait(false);

        return storeEntity == null ? null : _mapper.Map<StoreModel>(storeEntity);
    }
}
