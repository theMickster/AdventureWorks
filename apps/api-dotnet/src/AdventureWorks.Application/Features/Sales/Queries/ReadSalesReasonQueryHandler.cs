using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesReasonQueryHandler(
    IMapper mapper,
    ISalesReasonRepository salesReasonRepository)
        : IRequestHandler<ReadSalesReasonQuery, SalesReasonModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesReasonRepository _repository = salesReasonRepository ?? throw new ArgumentNullException(nameof(salesReasonRepository));

    public async Task<SalesReasonModel?> Handle(ReadSalesReasonQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<SalesReasonModel>(await _repository.GetByIdAsync(request.Id, cancellationToken));
    }
}
