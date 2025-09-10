using AdventureWorks.Application.Features.Person.Validators;
using AdventureWorks.Models.Features.Person;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Person.Validators;

[ExcludeFromCodeCoverage]
public sealed class PersonPhoneUpdateValidatorTests : UnitTestBase
{
    private readonly PersonPhoneUpdateValidator _sut = new();

    [Fact]
    public void Validator_succeeds_when_input_is_valid()
    {
        var model = new PersonPhoneUpdateModel { PhoneNumber = "555-000-0002" };

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_should_have_Rule01_when_phone_number_is_empty()
    {
        var model = new PersonPhoneUpdateModel { PhoneNumber = string.Empty };

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public void Validator_should_have_Rule02_when_phone_number_exceeds_25_characters()
    {
        var model = new PersonPhoneUpdateModel { PhoneNumber = new string('9', 26) };

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorCode("Rule-02");
    }
}
