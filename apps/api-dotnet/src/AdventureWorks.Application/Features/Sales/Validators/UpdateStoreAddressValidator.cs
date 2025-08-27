using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validates the payload for updating a store address.
/// Inherits the <c>AddressTypeId</c> rule (Rule-01) from <see cref="StoreAddressBaseModelValidator{T}"/>.
/// Rule-02 (duplicate composite key) is enforced in the handler because it depends on the route storeId/addressId.
/// </summary>
public sealed class UpdateStoreAddressValidator : StoreAddressBaseModelValidator<StoreAddressUpdateModel>
{
    public UpdateStoreAddressValidator(IAddressTypeRepository addressTypeRepository)
        : base(addressTypeRepository)
    {
    }
}
