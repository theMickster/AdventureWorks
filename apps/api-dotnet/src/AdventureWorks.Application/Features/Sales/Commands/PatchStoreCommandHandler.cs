using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for PatchStoreCommand.
/// Applies JSON Patch operations to a store's information.
/// </summary>
public sealed class PatchStoreCommandHandler(
    IStoreRepository storeRepository,
    IValidator<StoreUpdateModel> validator)
        : IRequestHandler<PatchStoreCommand, Unit>
{
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IValidator<StoreUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Applies the JSON Patch operations to the store and persists the result.
    /// </summary>
    /// <param name="request">The patch command containing the store ID and patch document.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task<Unit> Handle(PatchStoreCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.PatchDocument);

        var entity = await _storeRepository.GetByIdAsync(request.StoreId);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Store with ID {request.StoreId} not found.");
        }

        var model = new StoreUpdateModel
        {
            Id = entity.BusinessEntityId,
            Name = entity.Name,
            SalesPersonId = entity.SalesPersonId
        };

        var patchErrors = new List<string>();
        request.PatchDocument.ApplyTo(model, error => patchErrors.Add(error.ErrorMessage));
        if (patchErrors.Count > 0)
        {
            throw new ValidationException(
                patchErrors.Select(e => new ValidationFailure("PatchDocument", e)));
        }

        if (model.Id != request.StoreId)
        {
            model.Id = request.StoreId;
        }

        await _validator.ValidateAndThrowAsync(model, cancellationToken);

        entity.Name = model.Name;
        entity.SalesPersonId = model.SalesPersonId;
        entity.ModifiedDate = request.ModifiedDate;

        await _storeRepository.UpdateAsync(entity);

        return Unit.Value;
    }
}
