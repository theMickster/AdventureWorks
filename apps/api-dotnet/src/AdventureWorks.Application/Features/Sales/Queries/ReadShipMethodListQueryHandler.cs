using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadShipMethodListQueryHandler(
    IMapper mapper,
    IShipMethodRepository shipMethodRepository)
        : IRequestHandler<ReadShipMethodListQuery, List<ShipMethodModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IShipMethodRepository _repository = shipMethodRepository ?? throw new ArgumentNullException(nameof(shipMethodRepository));

    public async Task<List<ShipMethodModel>> Handle(ReadShipMethodListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<ShipMethodModel>>(entities);
    }
}
