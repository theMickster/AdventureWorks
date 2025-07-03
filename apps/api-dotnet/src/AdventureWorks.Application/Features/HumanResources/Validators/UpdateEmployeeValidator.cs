using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Validator for EmployeeUpdateModel with validation for employee update operations.
/// </summary>
public sealed class UpdateEmployeeValidator : AbstractValidator<EmployeeUpdateModel>
{
    private readonly IEmployeeRepository _employeeRepository;

    public UpdateEmployeeValidator(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

        // ID validation
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode("Rule-01")
            .WithMessage(MessageIdGreaterThanZero)
            .MustAsync(async (id, cancellation) => await EmployeeMustExistAsync(id))
            .WithErrorCode("Rule-02")
            .WithMessage(MessageEmployeeIdExists);

        // FirstName validation
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode("Rule-03")
            .WithMessage(MessageFirstNameRequired)
            .MaximumLength(50)
            .WithErrorCode("Rule-04")
            .WithMessage(MessageFirstNameLength);

        // LastName validation
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode("Rule-05")
            .WithMessage(MessageLastNameRequired)
            .MaximumLength(50)
            .WithErrorCode("Rule-06")
            .WithMessage(MessageLastNameLength);

        // MiddleName validation
        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .WithErrorCode("Rule-07")
            .WithMessage(MessageMiddleNameLength)
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        // Title validation
        RuleFor(x => x.Title)
            .MaximumLength(8)
            .WithErrorCode("Rule-08")
            .WithMessage(MessageTitleLength)
            .When(x => !string.IsNullOrEmpty(x.Title));

        // Suffix validation
        RuleFor(x => x.Suffix)
            .MaximumLength(10)
            .WithErrorCode("Rule-09")
            .WithMessage(MessageSuffixLength)
            .When(x => !string.IsNullOrEmpty(x.Suffix));

        // MaritalStatus validation
        RuleFor(x => x.MaritalStatus)
            .Must(status => status == "M" || status == "S")
            .WithErrorCode("Rule-10")
            .WithMessage(MessageMaritalStatusInvalid);

        // Gender validation
        RuleFor(x => x.Gender)
            .Must(gender => gender == "M" || gender == "F")
            .WithErrorCode("Rule-11")
            .WithMessage(MessageGenderInvalid);
    }

    public static string MessageIdGreaterThanZero => "Employee ID must be greater than 0";
    public static string MessageEmployeeIdExists => "Employee ID must exist prior to update";
    public static string MessageFirstNameRequired => "First name is required";
    public static string MessageFirstNameLength => "First name cannot be greater than 50 characters";
    public static string MessageLastNameRequired => "Last name is required";
    public static string MessageLastNameLength => "Last name cannot be greater than 50 characters";
    public static string MessageMiddleNameLength => "Middle name cannot be greater than 50 characters";
    public static string MessageTitleLength => "Title cannot be greater than 8 characters";
    public static string MessageSuffixLength => "Suffix cannot be greater than 10 characters";
    public static string MessageMaritalStatusInvalid => "Marital status must be 'M' (Married) or 'S' (Single)";
    public static string MessageGenderInvalid => "Gender must be 'M' (Male) or 'F' (Female)";

    private async Task<bool> EmployeeMustExistAsync(int employeeId)
    {
        var result = await _employeeRepository.GetEmployeeByIdAsync(employeeId).ConfigureAwait(false);
        return result != null && result.BusinessEntityId != int.MinValue;
    }
}
