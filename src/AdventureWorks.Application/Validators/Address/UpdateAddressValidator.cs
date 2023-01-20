using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Domain.Models;
using FluentValidation;

namespace AdventureWorks.Application.Validators.Address;

public sealed class UpdateAddressValidator : AddressBaseModelValidator<AddressUpdateModel>
{
    public UpdateAddressValidator(IStateProvinceRepository stateProvinceRepository):
        base(stateProvinceRepository)
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage(AddressIdValidInteger)
            .WithErrorCode("Rule-09")
            .GreaterThan(0)
            .WithMessage(AddressIdValidInteger)
            .WithErrorCode("Rule-09");
    }

    public static string AddressIdValidInteger => "Address Id must be a positive integer.";
        
}