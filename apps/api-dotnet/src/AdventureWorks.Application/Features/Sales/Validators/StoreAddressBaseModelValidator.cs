using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Base validator for <see cref="StoreAddressBaseModel"/>.
/// Owns the shared <c>AddressTypeId</c> rule (Rule-01) so Create and Update validators
/// stay consistent in error code, message, and FK existence check.
/// </summary>
public abstract class StoreAddressBaseModelValidator<T> : AbstractValidator<T> where T : StoreAddressBaseModel
{
    protected StoreAddressBaseModelValidator(IAddressTypeRepository addressTypeRepository)
    {
        ArgumentNullException.ThrowIfNull(addressTypeRepository);

        RuleFor(x => x.AddressTypeId)
            .GreaterThan(0)
            .WithErrorCode("Rule-01").WithMessage(MessageAddressTypeIdInvalid)
            .MustAsync(async (id, ct) => await addressTypeRepository.ExistsAsync(id, ct))
            .WithErrorCode("Rule-01").WithMessage(MessageAddressTypeIdInvalid);
    }

    public static string MessageAddressTypeIdInvalid => "The specified address type does not exist.";
}
