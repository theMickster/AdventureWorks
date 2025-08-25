using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validates the payload for updating a store contact.
/// Inherits the <c>ContactTypeId</c> rule (Rule-01) from <see cref="StoreContactBaseModelValidator{T}"/>.
/// Rule-02 (duplicate composite key) is enforced in the handler because it depends on the route storeId/personId.
/// </summary>
public sealed class UpdateStoreContactValidator : StoreContactBaseModelValidator<StoreContactUpdateModel>
{
    public UpdateStoreContactValidator(IContactTypeEntityRepository contactTypeRepository)
        : base(contactTypeRepository)
    {
    }
}
