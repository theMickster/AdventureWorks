using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Commands;

public sealed class CreateProductCommandHandler(
    IMapper mapper,
    IProductRepository productRepository,
    IValidator<ProductCreateModel> validator)
        : IRequestHandler<CreateProductCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    private readonly IValidator<ProductCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var inputEntity = _mapper.Map<Product>(request.Model);
        inputEntity.ModifiedDate = request.ModifiedDate;
        inputEntity.Rowguid = request.RowGuid;

        var outputEntity = await _productRepository.AddAsync(inputEntity, cancellationToken);

        return outputEntity.ProductId;
    }
}
