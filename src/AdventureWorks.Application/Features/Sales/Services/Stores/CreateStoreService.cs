using AdventureWorks.Application.Features.Sales.Contracts;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Services.Stores;

[ServiceLifetimeScoped]
public sealed class CreateStoreService : ICreateStoreService
{
    private readonly IMapper _mapper;
    private readonly IStoreRepository _storeRepository;

    public CreateStoreService(
        IMapper mapper,
        IStoreRepository storeRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    }
}
