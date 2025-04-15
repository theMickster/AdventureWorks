using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesTerritoryListQueryHandler(
    IMapper mapper,
    ISalesTerritoryRepository salesTerritoryRepository)
    : IRequestHandler<ReadSalesTerritoryListQuery, List<SalesTerritoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesTerritoryRepository _repository = salesTerritoryRepository ?? throw new ArgumentNullException(nameof(salesTerritoryRepository));

    public async Task<List<SalesTerritoryModel>> Handle(ReadSalesTerritoryListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<SalesTerritoryModel>>(entities);
    }
}
