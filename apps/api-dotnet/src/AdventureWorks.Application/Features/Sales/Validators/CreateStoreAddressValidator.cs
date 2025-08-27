using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validates the payload for creating a new store address.
/// Inherits the <c>AddressTypeId</c> rule (Rule-01) from <see cref="StoreAddressBaseModelValidator{T}"/>
/// and adds the <c>AddressId</c> rule (Rule-02). Rule-03 (duplicate composite key) is enforced in the handler
/// because it depends on the route storeId.
/// </summary>
public sealed class CreateStoreAddressValidator : StoreAddressBaseModelValidator<StoreAddressCreateModel>
{
    public CreateStoreAddressValidator(
        IAddressRepository addressRepository,
        IAddressTypeRepository addressTypeRepository)
            : base(addressTypeRepository)
    {
        ArgumentNullException.ThrowIfNull(addressRepository);

        RuleFor(x => x.AddressId)
            .GreaterThan(0)
            .WithErrorCode("Rule-02").WithMessage(MessageAddressIdInvalid)
            .MustAsync(async (id, ct) => await addressRepository.ExistsAsync(id, ct))
            .WithErrorCode("Rule-02").WithMessage(MessageAddressIdInvalid);
    }

    public static string MessageAddressIdInvalid => "The specified address does not exist.";
}
