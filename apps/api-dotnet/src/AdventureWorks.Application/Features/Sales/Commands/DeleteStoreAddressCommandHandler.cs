using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="DeleteStoreAddressCommand"/>.
/// Locates the address by composite key and deletes it. Throws <see cref="KeyNotFoundException"/> if missing.
/// </summary>
public sealed class DeleteStoreAddressCommandHandler(
    IBusinessEntityAddressRepository businessEntityAddressRepository)
        : IRequestHandler<DeleteStoreAddressCommand, Unit>
{
    private readonly IBusinessEntityAddressRepository _businessEntityAddressRepository = businessEntityAddressRepository ?? throw new ArgumentNullException(nameof(businessEntityAddressRepository));

    public async Task<Unit> Handle(DeleteStoreAddressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _businessEntityAddressRepository.GetByCompositeKeyAsync(
            request.StoreId, request.AddressId, request.AddressTypeId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Store address not found for StoreId={request.StoreId}, AddressId={request.AddressId}, AddressTypeId={request.AddressTypeId}.");
        }

        await _businessEntityAddressRepository.DeleteAsync(existing, cancellationToken);

        return Unit.Value;
    }
}
