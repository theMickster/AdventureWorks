using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using FluentValidation;

namespace AdventureWorks.Application.Features.AddressManagement.Validators;

public class AddressBaseModelValidator<T> : AbstractValidator<T> where T : AddressBaseModel
{
    protected readonly IStateProvinceRepository StateProvinceRepository;

    public AddressBaseModelValidator(IStateProvinceRepository stateProvinceRepository)
    {
        StateProvinceRepository = stateProvinceRepository ?? throw new ArgumentNullException(nameof(stateProvinceRepository));

        RuleFor(a => a.AddressLine1)
            .NotEmpty()
            .WithErrorCode("Rule-01").WithMessage(MessageAddressLine1Empty)
            .MaximumLength(60)
            .WithErrorCode("Rule-02").WithMessage(MessageAddressLine1Length);

        RuleFor(a => a.AddressLine2)
            .MaximumLength(60)
            .WithErrorCode("Rule-03").WithMessage(MessageAddressLine2Length);

        RuleFor(a => a.City)
            .NotEmpty()
            .WithErrorCode("Rule-04").WithMessage(MessageCityEmpty)
            .MaximumLength(30)
            .WithErrorCode("Rule-05").WithMessage(MessageCityLength);

        RuleFor(a => a.PostalCode)
            .NotEmpty()
            .WithErrorCode("Rule-05").WithMessage(PostalCodeEmpty)
            .MaximumLength(15)
            .WithErrorCode("Rule-06").WithMessage(PostalCodeLength);

        RuleFor(address => address)
            .MustAsync(async (address, cancellation)
                => await StateProvinceMustExistAsync(address.AddressStateProvince.Id).ConfigureAwait(false))
            .When(x => x?.AddressStateProvince != null)
            .WithMessage(StateProvinceIdExists)
            .WithErrorCode("Rule-07")
            .OverridePropertyName("AddressStateProvince");

        RuleFor(x => x.AddressStateProvince)
            .NotNull()
            .WithMessage(StateProvinceExists)
            .WithErrorCode("Rule-08")
            .OverridePropertyName("AddressStateProvince");
    }

    public static string MessageAddressLine1Empty => "Address Line 1 cannot be null, empty, or whitespace";

    public static string MessageAddressLine1Length => "Address Line 1 cannot be greater than 60 characters";

    public static string MessageAddressLine2Length => "Address Line 2 cannot be greater than 60 characters";

    public static string MessageCityEmpty => "City cannot be null, empty, or whitespace";

    public static string MessageCityLength => "City cannot be greater than 30 characters";

    public static string PostalCodeEmpty => "Postal Code cannot be null, empty, or whitespace";

    public static string PostalCodeLength => "Postal Code cannot be greater than 15 characters";

    public static string StateProvinceIdExists => "AddressStateProvince Id must exist prior to use";

    public static string StateProvinceExists => "AddressStateProvince is required";

    protected async Task<bool> StateProvinceMustExistAsync(int stateProvinceId)
    {
        var result = await StateProvinceRepository.GetByIdAsync(stateProvinceId).ConfigureAwait(false);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        return result != null && result.StateProvinceId != int.MinValue;
    }
}