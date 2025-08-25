using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validates the payload for creating a new store contact.
/// Inherits the <c>ContactTypeId</c> rule (Rule-01) from <see cref="StoreContactBaseModelValidator{T}"/>
/// and adds the <c>PersonId</c> rule (Rule-02). Rule-03 (duplicate composite key) is enforced in the handler
/// because it depends on the route storeId.
/// </summary>
public sealed class CreateStoreContactValidator : StoreContactBaseModelValidator<StoreContactCreateModel>
{
    public CreateStoreContactValidator(
        IPersonRepository personRepository,
        IContactTypeEntityRepository contactTypeRepository)
            : base(contactTypeRepository)
    {
        ArgumentNullException.ThrowIfNull(personRepository);

        RuleFor(x => x.PersonId)
            .GreaterThan(0)
            .WithErrorCode("Rule-02").WithMessage(MessagePersonIdInvalid)
            .MustAsync(async (id, ct) => await personRepository.ExistsAsync(id, ct))
            .WithErrorCode("Rule-02").WithMessage(MessagePersonIdInvalid);
    }

    public static string MessagePersonIdInvalid => "The specified person does not exist.";
}
