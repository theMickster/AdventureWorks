using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.Application.Features.AddressManagement.Validators;

public sealed class CreateAddressValidator : AddressBaseModelValidator<AddressCreateModel>
{
    public CreateAddressValidator(IStateProvinceRepository stateProvinceRepository)
        :base(stateProvinceRepository)
    {
    }
}