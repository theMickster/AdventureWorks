using AdventureWorks.Application.Features.AddressManagement.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validator for CreateSalesPersonModel with comprehensive validation for sales person creation.
/// Validates Person, Employee, and SalesPerson-specific fields.
/// </summary>
public sealed class CreateSalesPersonValidator : SalesPersonBaseModelValidator<SalesPersonCreateModel>
{
    private readonly IPhoneNumberTypeRepository _phoneNumberTypeRepository;
    private readonly IStateProvinceRepository _stateProvinceRepository;
    private readonly IAddressTypeRepository _addressTypeRepository;

    public CreateSalesPersonValidator(
        IPhoneNumberTypeRepository phoneNumberTypeRepository,
        IStateProvinceRepository stateProvinceRepository,
        IAddressTypeRepository addressTypeRepository)
    {
        _phoneNumberTypeRepository = phoneNumberTypeRepository ?? throw new ArgumentNullException(nameof(phoneNumberTypeRepository));
        _stateProvinceRepository = stateProvinceRepository ?? throw new ArgumentNullException(nameof(stateProvinceRepository));
        _addressTypeRepository = addressTypeRepository ?? throw new ArgumentNullException(nameof(addressTypeRepository));

        // Person name validations
        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithErrorCode("Rule-10")
            .WithMessage(MessageFirstNameLength);

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithErrorCode("Rule-11")
            .WithMessage(MessageLastNameLength);

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName))
            .WithErrorCode("Rule-12")
            .WithMessage(MessageMiddleNameLength);

        RuleFor(x => x.Title)
            .MaximumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithErrorCode("Rule-13")
            .WithMessage(MessageTitleLength);

        RuleFor(x => x.Suffix)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Suffix))
            .WithErrorCode("Rule-14")
            .WithMessage(MessageSuffixLength);

        // National ID validation
        RuleFor(x => x.NationalIdNumber)
            .MaximumLength(15)
            .WithErrorCode("Rule-15")
            .WithMessage(MessageNationalIdNumberLength);

        // Login ID validation
        RuleFor(x => x.LoginId)
            .MaximumLength(256)
            .WithErrorCode("Rule-16")
            .WithMessage(MessageLoginIdLength);

        // Job Title validation
        RuleFor(x => x.JobTitle)
            .MaximumLength(50)
            .WithErrorCode("Rule-17")
            .WithMessage(MessageJobTitleLength);

        // Marital status validation
        RuleFor(x => x.MaritalStatus)
            .Must(x => x == "M" || x == "S")
            .WithErrorCode("Rule-18")
            .WithMessage(MessageMaritalStatusInvalid);

        // Gender validation
        RuleFor(x => x.Gender)
            .Must(x => x == "M" || x == "F")
            .WithErrorCode("Rule-19")
            .WithMessage(MessageGenderInvalid);

        // Organization level validation
        RuleFor(x => x.OrganizationLevel)
            .GreaterThanOrEqualTo((short)0)
            .When(x => x.OrganizationLevel.HasValue)
            .WithErrorCode("Rule-20")
            .WithMessage(MessageOrganizationLevelInvalid);

        // Birth date validation
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithErrorCode("Rule-21")
            .WithMessage(MessageBirthDateEmpty)
            .LessThan(DateTime.Today.AddYears(-18))
            .WithErrorCode("Rule-22")
            .WithMessage(MessageBirthDateMinimumAge);

        // Hire date validation
        RuleFor(x => x.HireDate)
            .NotEmpty()
            .WithErrorCode("Rule-23")
            .WithMessage(MessageHireDateEmpty)
            .LessThanOrEqualTo(DateTime.Today)
            .WithErrorCode("Rule-24")
            .WithMessage(MessageHireDateFuture);

        // Hire date must be after birth date
        RuleFor(x => x)
            .Must(model => model.HireDate > model.BirthDate)
            .WithErrorCode("Rule-25")
            .WithMessage(MessageHireDateAfterBirthDate)
            .OverridePropertyName(nameof(SalesPersonCreateModel.HireDate));

        // Phone validation
        RuleFor(x => x.Phone)
            .SetValidator(new SalesPersonPhoneValidator(_phoneNumberTypeRepository)!);

        // Email validation
        RuleFor(x => x.EmailAddress)
            .EmailAddress()
            .WithErrorCode("Rule-26")
            .WithMessage(MessageEmailAddressInvalid)
            .MaximumLength(50)
            .WithErrorCode("Rule-27")
            .WithMessage(MessageEmailAddressLength);

        // Address validation
        RuleFor(x => x.Address)
            .SetValidator(new AddressBaseModelValidator<AddressCreateModel>(_stateProvinceRepository)!);

        // Address type validation
        RuleFor(x => x.AddressTypeId)
            .GreaterThan(0)
            .WithErrorCode("Rule-28")
            .WithMessage(MessageAddressTypeIdGreaterThanZero)
            .MustAsync(async (id, cancellation) => await AddressTypeMustExistAsync(id))
            .WithErrorCode("Rule-29")
            .WithMessage(MessageAddressTypeIdExists);
    }

    // Error messages
    public static string MessageFirstNameLength => "First name cannot be greater than 50 characters";
    public static string MessageLastNameLength => "Last name cannot be greater than 50 characters";
    public static string MessageMiddleNameLength => "Middle name cannot be greater than 50 characters";
    public static string MessageTitleLength => "Title cannot be greater than 8 characters";
    public static string MessageSuffixLength => "Suffix cannot be greater than 10 characters";
    public static string MessageNationalIdNumberLength => "National ID number cannot be greater than 15 characters";
    public static string MessageLoginIdLength => "Login ID cannot be greater than 256 characters";
    public static string MessageJobTitleLength => "Job title cannot be greater than 50 characters";
    public static string MessageMaritalStatusInvalid => "Marital status must be 'M' (Married) or 'S' (Single)";
    public static string MessageGenderInvalid => "Gender must be 'M' (Male) or 'F' (Female)";
    public static string MessageOrganizationLevelInvalid => "Organization level must be greater than or equal to 0";
    public static string MessageBirthDateEmpty => "Birth date cannot be null or empty";
    public static string MessageBirthDateMinimumAge => "Sales person must be at least 18 years old";
    public static string MessageHireDateEmpty => "Hire date cannot be null or empty";
    public static string MessageHireDateFuture => "Hire date cannot be in the future";
    public static string MessageHireDateAfterBirthDate => "Hire date must be after birth date";
    public static string MessageEmailAddressInvalid => "Email address must be in a valid format";
    public static string MessageEmailAddressLength => "Email address cannot be greater than 50 characters";
    public static string MessageAddressTypeIdGreaterThanZero => "Address type ID must be greater than 0";
    public static string MessageAddressTypeIdExists => "Address type ID must exist prior to use";

    private async Task<bool> AddressTypeMustExistAsync(int addressTypeId)
    {
        var result = await _addressTypeRepository.GetByIdAsync(addressTypeId).ConfigureAwait(false);
        return result != null && result.AddressTypeId != int.MinValue;
    }
}
