using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductCategoriesQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<ReadProductCategoriesQuery, List<ProductCategoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<List<ProductCategoryModel>> Handle(ReadProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _productRepository.GetProductCategoriesAsync(cancellationToken);

        return _mapper.Map<List<ProductCategoryModel>>(entities);
    }
}
