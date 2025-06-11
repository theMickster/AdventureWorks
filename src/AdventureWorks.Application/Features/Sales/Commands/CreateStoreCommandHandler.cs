using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class CreateStoreCommandHandler(
    IMapper mapper,
    IStoreRepository storeRepository,
    IValidator<StoreCreateModel> validator) 
        : IRequestHandler<CreateStoreCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IValidator<StoreCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var inputEntity = _mapper.Map<StoreEntity>(request.Model);
        inputEntity.ModifiedDate = request.ModifiedDate;
        inputEntity.Rowguid = request.RowGuid;

        var outputEntity = await _storeRepository.AddAsync(inputEntity);

        return outputEntity.BusinessEntityId;
    }
}
