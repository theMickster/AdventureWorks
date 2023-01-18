using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Domain.Models;

namespace AdventureWorks.Application.Validators.Address;

public sealed class CreateAddressValidator : AddressBaseModelValidator<AddressCreateModel>
{
    public CreateAddressValidator(IStateProvinceRepository stateProvinceRepository)
        :base(stateProvinceRepository)
    {
    }
}