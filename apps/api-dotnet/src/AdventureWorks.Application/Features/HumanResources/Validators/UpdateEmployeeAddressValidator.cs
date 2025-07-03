using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Validator for EmployeeAddressUpdateModel with validation for address update operations.
/// </summary>
public sealed class UpdateEmployeeAddressValidator : AbstractValidator<EmployeeAddressUpdateModel>
{
    private readonly IAddressRepository _addressRepository;
    private readonly IStateProvinceRepository _stateProvinceRepository;

    public UpdateEmployeeAddressValidator(
        IAddressRepository addressRepository,
        IStateProvinceRepository stateProvinceRepository)
    {
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _stateProvinceRepository = stateProvinceRepository ?? throw new ArgumentNullException(nameof(stateProvinceRepository));

        // AddressId validation
        RuleFor(x => x.AddressId)
            .GreaterThan(0)
            .WithErrorCode("Rule-01")
            .WithMessage(MessageAddressIdGreaterThanZero)
            .MustAsync(async (id, cancellation) => await AddressMustExistAsync(id))
            .WithErrorCode("Rule-02")
            .WithMessage(MessageAddressIdExists);

        // AddressLine1 validation
        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .WithErrorCode("Rule-03")
            .WithMessage(MessageAddressLine1Required)
            .MaximumLength(60)
            .WithErrorCode("Rule-04")
            .WithMessage(MessageAddressLine1Length);

        // AddressLine2 validation
        RuleFor(x => x.AddressLine2)
            .MaximumLength(60)
            .WithErrorCode("Rule-05")
            .WithMessage(MessageAddressLine2Length)
            .When(x => !string.IsNullOrEmpty(x.AddressLine2));

        // City validation
        RuleFor(x => x.City)
            .NotEmpty()
            .WithErrorCode("Rule-06")
            .WithMessage(MessageCityRequired)
            .MaximumLength(30)
            .WithErrorCode("Rule-07")
            .WithMessage(MessageCityLength);

        // StateProvinceId validation
        RuleFor(x => x.StateProvinceId)
            .GreaterThan(0)
            .WithErrorCode("Rule-08")
            .WithMessage(MessageStateProvinceIdGreaterThanZero)
            .MustAsync(async (id, cancellation) => await StateProvinceMustExistAsync(id))
            .WithErrorCode("Rule-09")
            .WithMessage(MessageStateProvinceIdExists);

        // PostalCode validation
        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithErrorCode("Rule-10")
            .WithMessage(MessagePostalCodeRequired)
            .MaximumLength(15)
            .WithErrorCode("Rule-11")
            .WithMessage(MessagePostalCodeLength);
    }

    public static string MessageAddressIdGreaterThanZero => "Address ID must be greater than 0";
    public static string MessageAddressIdExists => "Address ID must exist prior to update";
    public static string MessageAddressLine1Required => "Address line 1 is required";
    public static string MessageAddressLine1Length => "Address line 1 cannot be greater than 60 characters";
    public static string MessageAddressLine2Length => "Address line 2 cannot be greater than 60 characters";
    public static string MessageCityRequired => "City is required";
    public static string MessageCityLength => "City cannot be greater than 30 characters";
    public static string MessageStateProvinceIdGreaterThanZero => "State province ID must be greater than 0";
    public static string MessageStateProvinceIdExists => "State province ID must exist prior to use";
    public static string MessagePostalCodeRequired => "Postal code is required";
    public static string MessagePostalCodeLength => "Postal code cannot be greater than 15 characters";

    private async Task<bool> AddressMustExistAsync(int addressId)
    {
        var result = await _addressRepository.GetByIdAsync(addressId).ConfigureAwait(false);
        return result != null && result.AddressId != int.MinValue;
    }

    private async Task<bool> StateProvinceMustExistAsync(int stateProvinceId)
    {
        var result = await _stateProvinceRepository.GetByIdAsync(stateProvinceId).ConfigureAwait(false);
        return result != null && result.StateProvinceId != int.MinValue;
    }
}
