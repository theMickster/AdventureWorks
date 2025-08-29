using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadCurrencyListQueryHandler(
    IMapper mapper,
    ICurrencyRepository currencyRepository)
        : IRequestHandler<ReadCurrencyListQuery, List<CurrencyModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ICurrencyRepository _repository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));

    public async Task<List<CurrencyModel>> Handle(ReadCurrencyListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<CurrencyModel>>(entities);
    }
}
