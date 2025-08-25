using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="UpdateStoreContactTypeCommand"/>.
/// Validates the payload, locates the existing contact (404 if missing),
/// no-ops when target equals current, enforces uniqueness against the target composite key,
/// and replaces the row transactionally.
/// </summary>
public sealed class UpdateStoreContactTypeCommandHandler(
    IBusinessEntityContactEntityRepository businessEntityContactRepository,
    IValidator<StoreContactUpdateModel> validator)
        : IRequestHandler<UpdateStoreContactTypeCommand, int>
{
    private readonly IBusinessEntityContactEntityRepository _businessEntityContactRepository = businessEntityContactRepository ?? throw new ArgumentNullException(nameof(businessEntityContactRepository));
    private readonly IValidator<StoreContactUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(UpdateStoreContactTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var existing = await _businessEntityContactRepository.GetByCompositeKeyAsync(
            request.StoreId, request.PersonId, request.CurrentContactTypeId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Store contact not found for StoreId={request.StoreId}, PersonId={request.PersonId}, ContactTypeId={request.CurrentContactTypeId}.");
        }

        var targetContactTypeId = request.Model.ContactTypeId;

        if (targetContactTypeId != request.CurrentContactTypeId)
        {
            if (await _businessEntityContactRepository.ExistsAsync(request.StoreId, request.PersonId, targetContactTypeId, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(StoreContactUpdateModel.ContactTypeId), MessageDuplicateContact)
                    {
                        ErrorCode = "Rule-02"
                    }
                });
            }

            await _businessEntityContactRepository.ReplaceContactTypeAsync(existing, targetContactTypeId, request.ModifiedDate, cancellationToken);
        }

        return request.PersonId;
    }

    public static string MessageDuplicateContact => "A contact with the same person and target contact type already exists for this store.";
}
