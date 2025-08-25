using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Base validator for <see cref="StoreContactBaseModel"/>.
/// Owns the shared <c>ContactTypeId</c> rule (Rule-01) so Create and Update validators
/// stay consistent in error code, message, and FK existence check.
/// </summary>
public abstract class StoreContactBaseModelValidator<T> : AbstractValidator<T> where T : StoreContactBaseModel
{
    protected StoreContactBaseModelValidator(IContactTypeEntityRepository contactTypeRepository)
    {
        ArgumentNullException.ThrowIfNull(contactTypeRepository);

        RuleFor(x => x.ContactTypeId)
            .GreaterThan(0)
            .WithErrorCode("Rule-01").WithMessage(MessageContactTypeIdInvalid)
            .MustAsync(async (id, ct) => await contactTypeRepository.ExistsAsync(id, ct))
            .WithErrorCode("Rule-01").WithMessage(MessageContactTypeIdInvalid);
    }

    public static string MessageContactTypeIdInvalid => "The specified contact type does not exist.";
}
