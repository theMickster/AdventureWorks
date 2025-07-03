using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validator for SalesPersonPhoneCreateModel.
/// </summary>
public sealed class SalesPersonPhoneValidator : AbstractValidator<SalesPersonPhoneCreateModel>
{
    private readonly IPhoneNumberTypeRepository _phoneNumberTypeRepository;

    public SalesPersonPhoneValidator(IPhoneNumberTypeRepository phoneNumberTypeRepository)
    {
        _phoneNumberTypeRepository = phoneNumberTypeRepository ?? throw new ArgumentNullException(nameof(phoneNumberTypeRepository));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25)
            .WithErrorCode("Rule-30")
            .WithMessage(MessagePhoneNumberLength);

        RuleFor(x => x.PhoneNumberTypeId)
            .GreaterThan(0)
            .WithErrorCode("Rule-31")
            .WithMessage(MessagePhoneNumberTypeIdRequired)
            .MustAsync(async (id, cancellation) => await PhoneNumberTypeMustExistAsync(id))
            .WithErrorCode("Rule-32")
            .WithMessage(MessagePhoneNumberTypeIdExists);
    }

    public static string MessagePhoneNumberLength => "Phone number cannot be greater than 25 characters";
    public static string MessagePhoneNumberTypeIdRequired => "Phone number type ID must be greater than 0";
    public static string MessagePhoneNumberTypeIdExists => "Phone number type ID must exist prior to use";

    private async Task<bool> PhoneNumberTypeMustExistAsync(int phoneNumberTypeId)
    {
        var result = await _phoneNumberTypeRepository.GetByIdAsync(phoneNumberTypeId).ConfigureAwait(false);
        return result != null && result.PhoneNumberTypeId != int.MinValue;
    }
}
