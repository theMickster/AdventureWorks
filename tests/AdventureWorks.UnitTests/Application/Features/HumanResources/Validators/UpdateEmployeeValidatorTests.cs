using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class UpdateEmployeeValidatorTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly UpdateEmployeeValidator _sut;

    public UpdateEmployeeValidatorTests()
    {
        _sut = new UpdateEmployeeValidator(_mockEmployeeRepository.Object);

        // Setup default mock to return valid entity
        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidEmployeeEntity());
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            UpdateEmployeeValidator.MessageIdGreaterThanZero.Should().Be("Employee ID must be greater than 0");
            UpdateEmployeeValidator.MessageEmployeeIdExists.Should().Be("Employee ID must exist prior to update");
            UpdateEmployeeValidator.MessageFirstNameRequired.Should().Be("First name is required");
            UpdateEmployeeValidator.MessageFirstNameLength.Should().Be("First name cannot be greater than 50 characters");
            UpdateEmployeeValidator.MessageLastNameRequired.Should().Be("Last name is required");
            UpdateEmployeeValidator.MessageLastNameLength.Should().Be("Last name cannot be greater than 50 characters");
            UpdateEmployeeValidator.MessageMiddleNameLength.Should().Be("Middle name cannot be greater than 50 characters");
            UpdateEmployeeValidator.MessageTitleLength.Should().Be("Title cannot be greater than 8 characters");
            UpdateEmployeeValidator.MessageSuffixLength.Should().Be("Suffix cannot be greater than 10 characters");
            UpdateEmployeeValidator.MessageMaritalStatusInvalid.Should().Be("Marital status must be 'M' (Married) or 'S' (Single)");
            UpdateEmployeeValidator.MessageGenderInvalid.Should().Be("Gender must be 'M' (Male) or 'F' (Female)");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_id_greater_than_zero_error(int id)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.Id = id;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_should_have_employee_not_exists_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(999);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdAsync(999))
            .ReturnsAsync((EmployeeEntity?)null);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorCode("Rule-02");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validator_should_have_first_name_required_error(string firstName)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.FirstName = firstName!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task Validator_should_have_first_name_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.FirstName = new string('a', 51);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode("Rule-04");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validator_should_have_last_name_required_error(string lastName)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.LastName = lastName!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode("Rule-05");
    }

    [Fact]
    public async Task Validator_should_have_last_name_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.LastName = new string('a', 51);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode("Rule-06");
    }

    [Fact]
    public async Task Validator_should_have_middle_name_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.MiddleName = new string('a', 51);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.MiddleName)
            .WithErrorCode("Rule-07");
    }

    [Fact]
    public async Task Validator_should_have_title_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.Title = new string('a', 9);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorCode("Rule-08");
    }

    [Fact]
    public async Task Validator_should_have_suffix_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.Suffix = new string('a', 11);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Suffix)
            .WithErrorCode("Rule-09");
    }

    [Theory]
    [InlineData("X")]
    [InlineData("D")]
    [InlineData("Invalid")]
    public async Task Validator_should_have_marital_status_invalid_error(string maritalStatus)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.MaritalStatus = maritalStatus;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus)
            .WithErrorCode("Rule-10");
    }

    [Theory]
    [InlineData("X")]
    [InlineData("U")]
    [InlineData("Invalid")]
    public async Task Validator_should_have_gender_invalid_error(string gender)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.Gender = gender;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorCode("Rule-11");
    }

    [Fact]
    public async Task Validator_should_pass_with_valid_model()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_should_pass_with_null_optional_fields()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel();
        model.MiddleName = null;
        model.Title = null;
        model.Suffix = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
