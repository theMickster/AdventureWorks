using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class EmployeePhoneValidatorTests : UnitTestBase
{
    private readonly Mock<IPhoneNumberTypeRepository> _mockPhoneNumberTypeRepository = new();
    private readonly EmployeePhoneValidator _sut;

    public EmployeePhoneValidatorTests()
    {
        _sut = new EmployeePhoneValidator(_mockPhoneNumberTypeRepository.Object);

        // Setup default mock to return valid entity
        _mockPhoneNumberTypeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell" });
    }

    [Theory]
    [InlineData(26)] // 26 characters - too long
    [InlineData(50)] // 50 characters - too long
    public async Task Validator_should_have_phone_number_length_errorAsync(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidPhoneModel(new string('5', length));

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorCode("Rule-01");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_phone_number_type_id_required_errorAsync(int phoneNumberTypeId)
    {
        var model = HumanResourcesDomainFixtures.GetValidPhoneModel(phoneNumberTypeId: phoneNumberTypeId);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumberTypeId)
            .WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Validator_should_have_phone_number_type_id_exists_error_when_not_foundAsync()
    {
        _mockPhoneNumberTypeRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((PhoneNumberTypeEntity?)null);

        var model = HumanResourcesDomainFixtures.GetValidPhoneModel(phoneNumberTypeId: 999);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumberTypeId)
            .WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidPhoneModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("555-123-4567")]
    [InlineData("5551234567")]
    [InlineData("+1-555-123-4567")]
    [InlineData("(555) 123-4567")]
    public async Task Validator_accepts_various_valid_phone_formatsAsync(string phoneNumber)
    {
        var model = HumanResourcesDomainFixtures.GetValidPhoneModel(phoneNumber);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
