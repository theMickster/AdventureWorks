using AdventureWorks.Application.Features.Person.Validators;
using AdventureWorks.Models.Features.Person;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Person.Validators;

[ExcludeFromCodeCoverage]
public sealed class PersonPhoneCreateValidatorTests : UnitTestBase
{
    private readonly PersonPhoneCreateValidator _sut = new();

    [Fact]
    public void Validator_succeeds_when_input_is_valid()
    {
        var model = new PersonPhoneCreateModel { PhoneNumber = "555-000-0001", PhoneNumberTypeId = 1 };

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_should_have_Rule01_when_phone_number_is_empty()
    {
        var model = new PersonPhoneCreateModel { PhoneNumber = string.Empty, PhoneNumberTypeId = 1 };

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public void Validator_should_have_Rule02_when_phone_number_exceeds_25_characters()
    {
        var model = new PersonPhoneCreateModel { PhoneNumber = new string('1', 26), PhoneNumberTypeId = 1 };

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorCode("Rule-02");
    }
}
