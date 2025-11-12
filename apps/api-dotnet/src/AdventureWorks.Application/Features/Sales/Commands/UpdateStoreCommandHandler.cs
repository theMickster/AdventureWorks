using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class UpdateStoreCommandHandler(
    IMapper mapper,
    IStoreRepository storeRepository,
    IValidator<StoreUpdateModel> validator,
    IPublisher publisher)
        : IRequestHandler<UpdateStoreCommand>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IValidator<StoreUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);
        var currentEntity = await _storeRepository.GetByIdAsync(request.Model.Id, cancellationToken);
        if (currentEntity is null)
        {
            throw new KeyNotFoundException($"Store with ID {request.Model.Id} not found.");
        }
        _mapper.Map(request.Model, currentEntity);
        currentEntity.ModifiedDate = request.ModifiedDate;
        await _storeRepository.UpdateAsync(currentEntity, cancellationToken);

        await _publisher.Publish(new EntityChangedNotification
        {
            EntityType = "Store",
            EntityId = request.Model.Id,
            Action = "Updated",
            UserName = request.UserName,
            Timestamp = request.ModifiedDate
        }, cancellationToken);
    }
}
