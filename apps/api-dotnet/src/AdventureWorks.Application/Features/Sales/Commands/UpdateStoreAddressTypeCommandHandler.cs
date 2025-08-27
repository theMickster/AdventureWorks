using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="UpdateStoreAddressTypeCommand"/>.
/// Validates the payload, locates the existing address (404 if missing),
/// no-ops when target equals current, enforces uniqueness against the target composite key,
/// and replaces the row transactionally.
/// </summary>
public sealed class UpdateStoreAddressTypeCommandHandler(
    IBusinessEntityAddressRepository businessEntityAddressRepository,
    IValidator<StoreAddressUpdateModel> validator)
        : IRequestHandler<UpdateStoreAddressTypeCommand, int>
{
    private readonly IBusinessEntityAddressRepository _businessEntityAddressRepository = businessEntityAddressRepository ?? throw new ArgumentNullException(nameof(businessEntityAddressRepository));
    private readonly IValidator<StoreAddressUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(UpdateStoreAddressTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var existing = await _businessEntityAddressRepository.GetByCompositeKeyAsync(
            request.StoreId, request.AddressId, request.CurrentAddressTypeId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Store address not found for StoreId={request.StoreId}, AddressId={request.AddressId}, AddressTypeId={request.CurrentAddressTypeId}.");
        }

        var targetAddressTypeId = request.Model.AddressTypeId;

        if (targetAddressTypeId != request.CurrentAddressTypeId)
        {
            if (await _businessEntityAddressRepository.ExistsAsync(request.StoreId, request.AddressId, targetAddressTypeId, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(StoreAddressUpdateModel.AddressTypeId), MessageDuplicateAddress)
                    {
                        ErrorCode = "Rule-02"
                    }
                });
            }

            await _businessEntityAddressRepository.ReplaceAddressTypeAsync(existing, targetAddressTypeId, request.ModifiedDate, cancellationToken);
        }

        return request.AddressId;
    }

    public static string MessageDuplicateAddress => "An address with the same address and target address type already exists for this store.";
}
