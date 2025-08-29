using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSpecialOfferListQueryHandler(
    IMapper mapper,
    ISpecialOfferRepository specialOfferRepository)
        : IRequestHandler<ReadSpecialOfferListQuery, List<SpecialOfferModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISpecialOfferRepository _repository = specialOfferRepository ?? throw new ArgumentNullException(nameof(specialOfferRepository));

    public async Task<List<SpecialOfferModel>> Handle(ReadSpecialOfferListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<SpecialOfferModel>>(entities);
    }
}
