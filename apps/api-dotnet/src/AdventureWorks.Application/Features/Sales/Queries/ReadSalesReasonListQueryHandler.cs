using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesReasonListQueryHandler(
    IMapper mapper,
    ISalesReasonRepository salesReasonRepository)
        : IRequestHandler<ReadSalesReasonListQuery, List<SalesReasonModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesReasonRepository _repository = salesReasonRepository ?? throw new ArgumentNullException(nameof(salesReasonRepository));

    public async Task<List<SalesReasonModel>> Handle(ReadSalesReasonListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<SalesReasonModel>>(entities);
    }
}
