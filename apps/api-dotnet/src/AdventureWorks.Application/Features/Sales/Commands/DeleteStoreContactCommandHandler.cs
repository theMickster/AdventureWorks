using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="DeleteStoreContactCommand"/>.
/// Locates the contact by composite key and deletes it. Throws <see cref="KeyNotFoundException"/> if missing.
/// </summary>
public sealed class DeleteStoreContactCommandHandler(
    IBusinessEntityContactEntityRepository businessEntityContactRepository)
        : IRequestHandler<DeleteStoreContactCommand, Unit>
{
    private readonly IBusinessEntityContactEntityRepository _businessEntityContactRepository = businessEntityContactRepository ?? throw new ArgumentNullException(nameof(businessEntityContactRepository));

    public async Task<Unit> Handle(DeleteStoreContactCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _businessEntityContactRepository.GetByCompositeKeyAsync(
            request.StoreId, request.PersonId, request.ContactTypeId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Store contact not found for StoreId={request.StoreId}, PersonId={request.PersonId}, ContactTypeId={request.ContactTypeId}.");
        }

        await _businessEntityContactRepository.DeleteAsync(existing, cancellationToken);

        return Unit.Value;
    }
}
