using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validator for UpdateSalesPersonModel with validation for updatable Person/Employee/SalesPerson fields.
/// Note: Immutable fields (NationalIdNumber, LoginId, BirthDate, HireDate) are NOT included.
/// Note: Contact fields (Phone, Email, Address) are deferred to separate endpoint.
/// </summary>
public sealed class UpdateSalesPersonValidator : SalesPersonBaseModelValidator<SalesPersonUpdateModel>
{
    public UpdateSalesPersonValidator()
    {
        // ID validation
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode("Rule-11")
            .WithMessage(MessageIdRequired);

        // Person name validations
        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithErrorCode("Rule-10")
            .WithMessage(MessageFirstNameLength);

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithErrorCode("Rule-12")
            .WithMessage(MessageLastNameLength);

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName))
            .WithErrorCode("Rule-13")
            .WithMessage(MessageMiddleNameLength);

        RuleFor(x => x.Title)
            .MaximumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithErrorCode("Rule-14")
            .WithMessage(MessageTitleLength);

        RuleFor(x => x.Suffix)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Suffix))
            .WithErrorCode("Rule-15")
            .WithMessage(MessageSuffixLength);

        // Job Title validation
        RuleFor(x => x.JobTitle)
            .MaximumLength(50)
            .WithErrorCode("Rule-16")
            .WithMessage(MessageJobTitleLength);

        // Marital status validation
        RuleFor(x => x.MaritalStatus)
            .Must(x => x == "M" || x == "S")
            .WithErrorCode("Rule-17")
            .WithMessage(MessageMaritalStatusInvalid);

        // Gender validation
        RuleFor(x => x.Gender)
            .Must(x => x == "M" || x == "F")
            .WithErrorCode("Rule-18")
            .WithMessage(MessageGenderInvalid);

        // Organization level validation
        RuleFor(x => x.OrganizationLevel)
            .GreaterThanOrEqualTo((short)0)
            .When(x => x.OrganizationLevel.HasValue)
            .WithErrorCode("Rule-19")
            .WithMessage(MessageOrganizationLevelInvalid);
    }

    // Error messages
    public static string MessageIdRequired => "Sales Person ID must be greater than 0";
    public static string MessageFirstNameLength => "First name cannot be greater than 50 characters";
    public static string MessageLastNameLength => "Last name cannot be greater than 50 characters";
    public static string MessageMiddleNameLength => "Middle name cannot be greater than 50 characters";
    public static string MessageTitleLength => "Title cannot be greater than 8 characters";
    public static string MessageSuffixLength => "Suffix cannot be greater than 10 characters";
    public static string MessageJobTitleLength => "Job title cannot be greater than 50 characters";
    public static string MessageMaritalStatusInvalid => "Marital status must be 'M' (Married) or 'S' (Single)";
    public static string MessageGenderInvalid => "Gender must be 'M' (Male) or 'F' (Female)";
    public static string MessageOrganizationLevelInvalid => "Organization level must be greater than or equal to 0";
}
