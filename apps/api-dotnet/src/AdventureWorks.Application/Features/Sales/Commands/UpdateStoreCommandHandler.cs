using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class UpdateStoreCommandHandler (
    IMapper mapper,
    IStoreRepository storeRepository,
    IValidator<StoreUpdateModel> validator) 
        : IRequestHandler<UpdateStoreCommand>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IValidator<StoreUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    
    public async Task Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);
        var currentEntity = await _storeRepository.GetByIdAsync(request.Model.Id);
        _mapper.Map(request.Model, currentEntity);
        currentEntity.ModifiedDate = request.ModifiedDate;
        await _storeRepository.UpdateAsync(currentEntity);
    }
}
