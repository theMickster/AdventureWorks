using AdventureWorks.Application.Features.AddressManagement.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Validator for CreateEmployeeModel with comprehensive validation for employee creation.
/// </summary>
public sealed class CreateEmployeeValidator : EmployeeBaseModelValidator<EmployeeCreateModel>
{
    private readonly IPhoneNumberTypeRepository _phoneNumberTypeRepository;
    private readonly IStateProvinceRepository _stateProvinceRepository;
    private readonly IAddressTypeRepository _addressTypeRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public CreateEmployeeValidator(
        IPhoneNumberTypeRepository phoneNumberTypeRepository,
        IStateProvinceRepository stateProvinceRepository,
        IAddressTypeRepository addressTypeRepository,
        IEmployeeRepository employeeRepository)
    {
        _phoneNumberTypeRepository = phoneNumberTypeRepository ?? throw new ArgumentNullException(nameof(phoneNumberTypeRepository));
        _stateProvinceRepository = stateProvinceRepository ?? throw new ArgumentNullException(nameof(stateProvinceRepository));
        _addressTypeRepository = addressTypeRepository ?? throw new ArgumentNullException(nameof(addressTypeRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

        // National ID validation (NotEmpty handled by 'required' modifier)
        RuleFor(x => x.NationalIdNumber)
            .MaximumLength(15)
            .WithErrorCode("Rule-15")
            .WithMessage(MessageNationalIdNumberLength)
            .MustAsync(async (nationalIdNumber, cancellation) => await NationalIdNumberMustBeUniqueAsync(nationalIdNumber, cancellation))
            .WithErrorCode("Rule-31")
            .WithMessage(MessageNationalIdNumberUnique);

        // Login ID validation (NotEmpty handled by 'required' modifier)
        RuleFor(x => x.LoginId)
            .MaximumLength(256)
            .WithErrorCode("Rule-16")
            .WithMessage(MessageLoginIdLength)
            .MustAsync(async (loginId, cancellation) => await LoginIdMustBeUniqueAsync(loginId, cancellation))
            .WithErrorCode("Rule-32")
            .WithMessage(MessageLoginIdUnique);

        // Birth date validation
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithErrorCode("Rule-19")
            .WithMessage(MessageBirthDateEmpty)
            .LessThan(DateTime.Today.AddYears(-18))
            .WithErrorCode("Rule-20")
            .WithMessage(MessageBirthDateMinimumAge);

        // Phone validation (NotNull handled by 'required' modifier)
        RuleFor(x => x.Phone)
            .SetValidator(new EmployeePhoneValidator(_phoneNumberTypeRepository)!);

        // Email validation (NotEmpty handled by 'required' modifier)
        RuleFor(x => x.EmailAddress)
            .EmailAddress()
            .WithErrorCode("Rule-24")
            .WithMessage(MessageEmailAddressInvalid)
            .MaximumLength(50)
            .WithErrorCode("Rule-25")
            .WithMessage(MessageEmailAddressLength);

        // Address validation (NotNull handled by 'required' modifier)
        RuleFor(x => x.Address)
            .SetValidator(new AddressBaseModelValidator<AddressCreateModel>(_stateProvinceRepository)!);

        // Address type validation - REQUIRED
        RuleFor(x => x.AddressTypeId)
            .GreaterThan(0)
            .WithErrorCode("Rule-29")
            .WithMessage(MessageAddressTypeIdGreaterThanZero)
            .MustAsync(async (id, cancellation) => await AddressTypeMustExistAsync(id))
            .WithErrorCode("Rule-30")
            .WithMessage(MessageAddressTypeIdExists);
    }

    public static string MessageNationalIdNumberLength => "National ID number cannot be greater than 15 characters";
    public static string MessageLoginIdLength => "Login ID cannot be greater than 256 characters";
    public static string MessageBirthDateEmpty => "Birth date cannot be null or empty";
    public static string MessageBirthDateMinimumAge => "Employee must be at least 18 years old";
    public static string MessageEmailAddressInvalid => "Email address must be in a valid format";
    public static string MessageEmailAddressLength => "Email address cannot be greater than 50 characters";
    public static string MessageAddressTypeIdGreaterThanZero => "Address type ID must be greater than 0";
    public static string MessageAddressTypeIdExists => "Address type ID must exist prior to use";
    public static string MessageNationalIdNumberUnique => "An employee with this National ID number already exists";
    public static string MessageLoginIdUnique => "An employee with this Login ID already exists";

    private async Task<bool> AddressTypeMustExistAsync(int addressTypeId)
    {
        var result = await _addressTypeRepository.GetByIdAsync(addressTypeId).ConfigureAwait(false);
        return result != null && result.AddressTypeId != int.MinValue;
    }

    /// <summary>
    /// Primary uniqueness check for National ID Number (Rule-31). Catches the overwhelming majority
    /// of duplicates as a clean 400; a residual TOCTOU race window is closed by the repository-level
    /// DbUpdateException translation in <c>EmployeeRepository.CreateEmployeeWithPersonAsync</c> (409).
    /// </summary>
    private async Task<bool> NationalIdNumberMustBeUniqueAsync(string nationalIdNumber, CancellationToken cancellationToken)
    {
        var exists = await _employeeRepository.NationalIdNumberExistsAsync(nationalIdNumber, cancellationToken).ConfigureAwait(false);
        return !exists;
    }

    /// <summary>
    /// Primary uniqueness check for Login ID (Rule-32). See <see cref="NationalIdNumberMustBeUniqueAsync"/>
    /// for the race-condition rationale behind the repository-level backstop.
    /// </summary>
    private async Task<bool> LoginIdMustBeUniqueAsync(string loginId, CancellationToken cancellationToken)
    {
        var exists = await _employeeRepository.LoginIdExistsAsync(loginId, cancellationToken).ConfigureAwait(false);
        return !exists;
    }
}
