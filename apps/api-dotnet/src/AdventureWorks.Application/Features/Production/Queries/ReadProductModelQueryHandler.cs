using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductModelQueryHandler(
    IMapper mapper,
    IProductModelRepository productModelRepository)
        : IRequestHandler<ReadProductModelQuery, ProductModelDetailModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductModelRepository _repository = productModelRepository ?? throw new ArgumentNullException(nameof(productModelRepository));

    public async Task<ProductModelDetailModel?> Handle(ReadProductModelQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _mapper.Map<ProductModelDetailModel>(await _repository.GetByIdAsync(request.Id, cancellationToken));
    }
}
