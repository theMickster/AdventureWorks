using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.SalesTerritory;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Application.Services.SalesTerritory;

[ServiceLifetimeScoped]
public sealed class ReadSalesTerritoryService : IReadSalesTerritoryService
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

    /// <summary>
    /// Retrieve a sales territory using its identifier.
    /// </summary>
    /// <returns>A <see cref="SalesTerritoryModel"/> </returns>
    public async Task<SalesTerritoryModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);

        return _mapper.Map<SalesTerritoryModel>(entity);
    }

    /// <summary>
    /// Retrieve the list of sales territories. 
    /// </summary>
    /// <returns></returns>
    public async Task<List<SalesTerritoryModel>> GetListAsync()
    {
        var entities = await _repository.ListAllAsync().ConfigureAwait(false);

        if (entities == null || !entities.Any())
        {
            return new List<SalesTerritoryModel>();
        }

        return _mapper.Map<List<SalesTerritoryModel>>(entities);
    }
}
