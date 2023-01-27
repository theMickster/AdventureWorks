using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.SalesTerritory;
using AdventureWorks.Common.Attributes;
using AutoMapper;

namespace AdventureWorks.Application.Services.SalesTerritory;

[ServiceLifetimeScoped]
public sealed class ReadSalesTerritoryService : IReadSalesTerritory
{
    private readonly IMapper _mapper;
    private readonly ISalesTerritoryRepository _repository;

    public ReadSalesTerritoryService (
        IMapper mapper,
        ISalesTerritoryRepository salesTerritoryRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _repository = salesTerritoryRepository ?? throw new ArgumentNullException(nameof(salesTerritoryRepository));
    }


}
