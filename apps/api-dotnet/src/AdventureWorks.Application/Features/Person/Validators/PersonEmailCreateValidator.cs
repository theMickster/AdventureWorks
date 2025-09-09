using AdventureWorks.Models.Features.Person;
using FluentValidation;

namespace AdventureWorks.Application.Features.Person.Validators;

/// <summary>
/// Validates the payload for creating a new person email address.
/// Rule-01: email address null/empty. Rule-02: invalid email format. Rule-04: exceeds 50 chars.
/// Rule-03 (duplicate) is enforced in the command handler.
/// </summary>
public sealed class PersonEmailCreateValidator : AbstractValidator<PersonEmailCreateModel>
{
    public PersonEmailCreateValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithErrorCode("Rule-01")
            .WithMessage(MessageEmailAddressRequired)
            .EmailAddress()
            .WithErrorCode("Rule-02")
            .WithMessage(MessageEmailAddressInvalid)
            .MaximumLength(50)
            .WithErrorCode("Rule-04")
            .WithMessage(MessageEmailAddressTooLong);
    }

    public static string MessageEmailAddressRequired => "Email address is required.";
    public static string MessageEmailAddressInvalid => "A valid email address is required.";
    public static string MessageEmailAddressTooLong => "Email address must not exceed 50 characters.";
}
