using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductInventoryQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<ReadProductInventoryQuery, List<ProductInventoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<List<ProductInventoryModel>> Handle(ReadProductInventoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _productRepository.GetProductInventoryByProductIdAsync(request.ProductId, cancellationToken);

        return _mapper.Map<List<ProductInventoryModel>>(entities);
    }
}
