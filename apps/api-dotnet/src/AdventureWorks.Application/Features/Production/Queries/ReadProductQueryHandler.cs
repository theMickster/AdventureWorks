using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductQueryHandler(
    IMapper mapper,
    IProductRepository productRepository)
        : IRequestHandler<ReadProductQuery, ProductDetailModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

    public async Task<ProductDetailModel?> Handle(ReadProductQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _productRepository.GetProductByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return _mapper.Map<ProductDetailModel>(entity);
    }
}
