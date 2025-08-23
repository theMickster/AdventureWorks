using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Commands;

public sealed class UpdateProductCommandHandler(
    IMapper mapper,
    IProductRepository productRepository,
    IValidator<ProductUpdateModel> validator)
        : IRequestHandler<UpdateProductCommand>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    private readonly IValidator<ProductUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var currentEntity = await _productRepository.GetByIdAsync(request.Model.Id, cancellationToken);

        if (currentEntity == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.Model.Id} not found.");
        }

        _mapper.Map(request.Model, currentEntity);
        currentEntity.ModifiedDate = request.ModifiedDate;

        await _productRepository.UpdateAsync(currentEntity, cancellationToken);
    }
}
