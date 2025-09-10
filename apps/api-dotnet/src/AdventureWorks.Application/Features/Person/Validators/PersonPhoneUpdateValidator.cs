using AdventureWorks.Models.Features.Person;
using FluentValidation;

namespace AdventureWorks.Application.Features.Person.Validators;

/// <summary>
/// Validates the payload for updating a person phone number.
/// Rule-01: phone number null/empty. Rule-02: exceeds 25 chars.
/// Rule-03 (bad typeId) and Rule-04 (duplicate combo) are enforced in the command handler.
/// </summary>
public sealed class PersonPhoneUpdateValidator : AbstractValidator<PersonPhoneUpdateModel>
{
    public PersonPhoneUpdateValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithErrorCode("Rule-01")
            .WithMessage(MessagePhoneNumberRequired)
            .MaximumLength(25)
            .WithErrorCode("Rule-02")
            .WithMessage(MessagePhoneNumberTooLong);
    }

    public static string MessagePhoneNumberRequired => "Phone number is required.";
    public static string MessagePhoneNumberTooLong => "Phone number must not exceed 25 characters.";
}
