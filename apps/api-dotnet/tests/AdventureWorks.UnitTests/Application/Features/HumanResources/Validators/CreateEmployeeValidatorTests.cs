using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class CreateEmployeeValidatorTests : UnitTestBase
{
    private readonly Mock<IPhoneNumberTypeRepository> _mockPhoneNumberTypeRepository = new();
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private readonly Mock<IAddressTypeRepository> _mockAddressTypeRepository = new();
    private readonly CreateEmployeeValidator _sut;

    public CreateEmployeeValidatorTests()
    {
        _sut = new CreateEmployeeValidator(
            _mockPhoneNumberTypeRepository.Object,
            _mockStateProvinceRepository.Object,
            _mockAddressTypeRepository.Object);

        // Setup default mocks to return valid entities
        _mockPhoneNumberTypeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell" });

        _mockStateProvinceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new StateProvinceEntity { StateProvinceId = 79, Name = "Washington" });

        _mockAddressTypeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AddressTypeEntity { AddressTypeId = 2, Name = "Work" });
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            // CreateEmployeeValidator messages
            CreateEmployeeValidator.MessageNationalIdNumberLength.Should().Be("National ID number cannot be greater than 15 characters");
            CreateEmployeeValidator.MessageLoginIdLength.Should().Be("Login ID cannot be greater than 256 characters");
            CreateEmployeeValidator.MessageBirthDateEmpty.Should().Be("Birth date cannot be null or empty");
            CreateEmployeeValidator.MessageBirthDateMinimumAge.Should().Be("Employee must be at least 18 years old");
            CreateEmployeeValidator.MessageEmailAddressInvalid.Should().Be("Email address must be in a valid format");
            CreateEmployeeValidator.MessageEmailAddressLength.Should().Be("Email address cannot be greater than 50 characters");
            CreateEmployeeValidator.MessageAddressTypeIdGreaterThanZero.Should().Be("Address type ID must be greater than 0");
            CreateEmployeeValidator.MessageAddressTypeIdExists.Should().Be("Address type ID must exist prior to use");

            // EmployeeBaseModelValidator messages
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageFirstNameLength.Should().Be("First name cannot be greater than 50 characters");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageLastNameLength.Should().Be("Last name cannot be greater than 50 characters");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageMiddleNameLength.Should().Be("Middle name cannot be greater than 50 characters");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageTitleLength.Should().Be("Title cannot be greater than 8 characters");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageSuffixLength.Should().Be("Suffix cannot be greater than 10 characters");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageJobTitleLength.Should().Be("Job title cannot be greater than 50 characters");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageMaritalStatusInvalid.Should().Be("Marital status must be 'M' (Married) or 'S' (Single)");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageGenderInvalid.Should().Be("Gender must be 'M' (Male) or 'F' (Female)");
            EmployeeBaseModelValidator<EmployeeCreateModel>.MessageOrganizationLevelInvalid.Should().Be("Organization level must be greater than or equal to 0");

            // EmployeePhoneValidator messages
            EmployeePhoneValidator.MessagePhoneNumberLength.Should().Be("Phone number cannot be greater than 25 characters");
            EmployeePhoneValidator.MessagePhoneNumberTypeIdRequired.Should().Be("Phone number type ID must be greater than 0");
            EmployeePhoneValidator.MessagePhoneNumberTypeIdExists.Should().Be("Phone number type ID must exist prior to use");
        }
    }

    [Theory]
    [InlineData("1234567890123456")] // 16 characters - too long
    [InlineData("12345678901234567890")] // 20 characters - too long
    public async Task Validator_should_have_national_id_number_length_errorAsync(string nationalIdNumber)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.NationalIdNumber = nationalIdNumber;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.NationalIdNumber)
            .WithErrorCode("Rule-15");
    }

    [Theory]
    [InlineData(257)] // 257 characters - too long
    [InlineData(300)] // 300 characters - too long
    public async Task Validator_should_have_login_id_length_errorAsync(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.LoginId = new string('a', length);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.LoginId)
            .WithErrorCode("Rule-16");
    }

    [Fact]
    public async Task Validator_should_have_birth_date_minimum_age_errorAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.BirthDate = DateTime.Today.AddYears(-17); // Only 17 years old

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
            .WithErrorCode("Rule-20");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@")]
    [InlineData("invalid")]
    public async Task Validator_should_have_email_address_format_errorAsync(string emailAddress)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.EmailAddress = emailAddress;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress)
            .WithErrorCode("Rule-24");
    }

    [Fact]
    public async Task Validator_should_have_email_address_length_errorAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.EmailAddress = new string('a', 45) + "@test.com"; // 54 characters - too long

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress)
            .WithErrorCode("Rule-25");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_address_type_id_error_when_not_positiveAsync(int addressTypeId)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.AddressTypeId = addressTypeId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressTypeId)
            .WithErrorCode("Rule-29");
    }

    [Fact]
    public async Task Validator_should_have_address_type_id_exists_error_when_not_foundAsync()
    {
        _mockAddressTypeRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((AddressTypeEntity?)null);

        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.AddressTypeId = 999;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressTypeId)
            .WithErrorCode("Rule-30");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_succeeds_when_optional_fields_are_nullAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.MiddleName = null;
        model.Title = null;
        model.Suffix = null;
        model.OrganizationLevel = null;
        model.Address.AddressLine2 = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
