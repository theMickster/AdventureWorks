using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesTerritoryQueryHandler(
    IMapper mapper,
    ISalesTerritoryRepository salesTerritoryRepository) 
        : IRequestHandler<ReadSalesTerritoryQuery, SalesTerritoryModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesTerritoryRepository _repository = salesTerritoryRepository ?? throw new ArgumentNullException(nameof(salesTerritoryRepository));

    public async Task<SalesTerritoryModel> Handle(ReadSalesTerritoryQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<SalesTerritoryModel>(await _repository.GetByIdAsync(request.Id));
    }
}
