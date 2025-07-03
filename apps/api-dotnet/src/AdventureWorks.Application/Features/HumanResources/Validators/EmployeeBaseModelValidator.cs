using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Base validator for employee models containing common validation rules.
/// </summary>
/// <typeparam name="T">Type of employee model</typeparam>
public class EmployeeBaseModelValidator<T> : AbstractValidator<T> where T : EmployeeBaseModel
{
    public EmployeeBaseModelValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode("Rule-01")
            .WithMessage(MessageFirstNameRequired)
            .MaximumLength(50)
            .WithErrorCode("Rule-02")
            .WithMessage(MessageFirstNameLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode("Rule-03")
            .WithMessage(MessageLastNameRequired)
            .MaximumLength(50)
            .WithErrorCode("Rule-04")
            .WithMessage(MessageLastNameLength);

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .WithErrorCode("Rule-05")
            .WithMessage(MessageMiddleNameLength)
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Title)
            .MaximumLength(8)
            .WithErrorCode("Rule-06")
            .WithMessage(MessageTitleLength)
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Suffix)
            .MaximumLength(10)
            .WithErrorCode("Rule-07")
            .WithMessage(MessageSuffixLength)
            .When(x => !string.IsNullOrEmpty(x.Suffix));

        RuleFor(x => x.JobTitle)
            .NotEmpty()
            .WithErrorCode("Rule-08")
            .WithMessage(MessageJobTitleRequired)
            .MaximumLength(50)
            .WithErrorCode("Rule-09")
            .WithMessage(MessageJobTitleLength);

        RuleFor(x => x.MaritalStatus)
            .Must(status => status == "M" || status == "S")
            .WithErrorCode("Rule-10")
            .WithMessage(MessageMaritalStatusInvalid);

        RuleFor(x => x.Gender)
            .Must(gender => gender == "M" || gender == "F")
            .WithErrorCode("Rule-11")
            .WithMessage(MessageGenderInvalid);

        RuleFor(x => x.OrganizationLevel)
            .GreaterThanOrEqualTo((short)0)
            .WithErrorCode("Rule-12")
            .WithMessage(MessageOrganizationLevelInvalid)
            .When(x => x.OrganizationLevel.HasValue);
    }

    public static string MessageFirstNameRequired => "First name is required";
    public static string MessageFirstNameLength => "First name cannot be greater than 50 characters";
    public static string MessageLastNameRequired => "Last name is required";
    public static string MessageLastNameLength => "Last name cannot be greater than 50 characters";
    public static string MessageMiddleNameLength => "Middle name cannot be greater than 50 characters";
    public static string MessageTitleLength => "Title cannot be greater than 8 characters";
    public static string MessageSuffixLength => "Suffix cannot be greater than 10 characters";
    public static string MessageJobTitleRequired => "Job title is required";
    public static string MessageJobTitleLength => "Job title cannot be greater than 50 characters";
    public static string MessageMaritalStatusInvalid => "Marital status must be 'M' (Married) or 'S' (Single)";
    public static string MessageGenderInvalid => "Gender must be 'M' (Male) or 'F' (Female)";
    public static string MessageOrganizationLevelInvalid => "Organization level must be greater than or equal to 0";
}
