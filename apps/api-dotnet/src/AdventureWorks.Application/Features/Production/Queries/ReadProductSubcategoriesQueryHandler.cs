using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductSubcategoriesQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<ReadProductSubcategoriesQuery, List<ProductSubcategoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<List<ProductSubcategoryModel>> Handle(ReadProductSubcategoriesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entities = await _productRepository.GetProductSubcategoriesAsync(request.CategoryId, cancellationToken);

        return _mapper.Map<List<ProductSubcategoryModel>>(entities);
    }
}
