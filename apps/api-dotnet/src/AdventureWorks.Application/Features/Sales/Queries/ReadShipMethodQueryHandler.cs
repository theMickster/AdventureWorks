using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadShipMethodQueryHandler(
    IMapper mapper,
    IShipMethodRepository shipMethodRepository)
        : IRequestHandler<ReadShipMethodQuery, ShipMethodModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IShipMethodRepository _repository = shipMethodRepository ?? throw new ArgumentNullException(nameof(shipMethodRepository));

    public async Task<ShipMethodModel?> Handle(ReadShipMethodQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<ShipMethodModel>(await _repository.GetByIdAsync(request.Id, cancellationToken));
    }
}
