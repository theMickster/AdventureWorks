using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class UpdateEmployeeAddressValidatorTests : UnitTestBase
{
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private readonly UpdateEmployeeAddressValidator _sut;

    public UpdateEmployeeAddressValidatorTests()
    {
        _sut = new UpdateEmployeeAddressValidator(
            _mockAddressRepository.Object,
            _mockStateProvinceRepository.Object);

        // Setup default mocks
        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(HumanResourcesDomainFixtures.GetValidAddressEntity());

        _mockStateProvinceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new StateProvinceEntity { StateProvinceId = 79, Name = "Washington" });
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            UpdateEmployeeAddressValidator.MessageAddressIdGreaterThanZero.Should().Be("Address ID must be greater than 0");
            UpdateEmployeeAddressValidator.MessageAddressIdExists.Should().Be("Address ID must exist prior to update");
            UpdateEmployeeAddressValidator.MessageAddressLine1Required.Should().Be("Address line 1 is required");
            UpdateEmployeeAddressValidator.MessageAddressLine1Length.Should().Be("Address line 1 cannot be greater than 60 characters");
            UpdateEmployeeAddressValidator.MessageAddressLine2Length.Should().Be("Address line 2 cannot be greater than 60 characters");
            UpdateEmployeeAddressValidator.MessageCityRequired.Should().Be("City is required");
            UpdateEmployeeAddressValidator.MessageCityLength.Should().Be("City cannot be greater than 30 characters");
            UpdateEmployeeAddressValidator.MessageStateProvinceIdGreaterThanZero.Should().Be("State province ID must be greater than 0");
            UpdateEmployeeAddressValidator.MessageStateProvinceIdExists.Should().Be("State province ID must exist prior to use");
            UpdateEmployeeAddressValidator.MessagePostalCodeRequired.Should().Be("Postal code is required");
            UpdateEmployeeAddressValidator.MessagePostalCodeLength.Should().Be("Postal code cannot be greater than 15 characters");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_address_id_greater_than_zero_error(int addressId)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.AddressId = addressId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressId)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_should_have_address_not_exists_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(999);

        _mockAddressRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((AddressEntity?)null);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressId)
            .WithErrorCode("Rule-02");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_should_have_address_line1_required_error(string addressLine1)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.AddressLine1 = addressLine1!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressLine1)
            .WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task Validator_should_have_address_line1_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.AddressLine1 = new string('a', 61);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressLine1)
            .WithErrorCode("Rule-04");
    }

    [Fact]
    public async Task Validator_should_have_address_line2_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.AddressLine2 = new string('a', 61);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressLine2)
            .WithErrorCode("Rule-05");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_should_have_city_required_error(string city)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.City = city!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorCode("Rule-06");
    }

    [Fact]
    public async Task Validator_should_have_city_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.City = new string('a', 31);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorCode("Rule-07");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_state_province_id_greater_than_zero_error(int stateProvinceId)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.StateProvinceId = stateProvinceId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.StateProvinceId)
            .WithErrorCode("Rule-08");
    }

    [Fact]
    public async Task Validator_should_have_state_province_not_exists_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.StateProvinceId = 999;

        _mockStateProvinceRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((StateProvinceEntity?)null);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.StateProvinceId)
            .WithErrorCode("Rule-09");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_should_have_postal_code_required_error(string postalCode)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.PostalCode = postalCode!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PostalCode)
            .WithErrorCode("Rule-10");
    }

    [Fact]
    public async Task Validator_should_have_postal_code_length_error()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.PostalCode = new string('a', 16);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PostalCode)
            .WithErrorCode("Rule-11");
    }

    [Fact]
    public async Task Validator_should_pass_with_valid_model()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_should_pass_with_null_address_line2()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel();
        model.AddressLine2 = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
