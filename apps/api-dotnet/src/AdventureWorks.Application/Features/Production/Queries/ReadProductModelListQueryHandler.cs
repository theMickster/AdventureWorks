using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductModelListQueryHandler(
    IMapper mapper,
    IProductModelRepository productModelRepository)
        : IRequestHandler<ReadProductModelListQuery, List<ProductModelListModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductModelRepository _repository = productModelRepository ?? throw new ArgumentNullException(nameof(productModelRepository));

    public async Task<List<ProductModelListModel>> Handle(ReadProductModelListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _repository.ListAllAsync(cancellationToken);
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<ProductModelListModel>>(entities);
    }
}
