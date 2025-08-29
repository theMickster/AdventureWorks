using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadCurrencyQueryHandler(
    IMapper mapper,
    ICurrencyRepository currencyRepository)
        : IRequestHandler<ReadCurrencyQuery, CurrencyModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ICurrencyRepository _repository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));

    public async Task<CurrencyModel?> Handle(ReadCurrencyQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<CurrencyModel>(await _repository.GetByIdAsync(request.Code, cancellationToken));
    }
}
