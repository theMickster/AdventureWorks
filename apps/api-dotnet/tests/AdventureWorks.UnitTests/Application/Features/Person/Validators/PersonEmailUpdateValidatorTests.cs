using AdventureWorks.Application.Features.Person.Validators;
using AdventureWorks.Models.Features.Person;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Person.Validators;

[ExcludeFromCodeCoverage]
public sealed class PersonEmailUpdateValidatorTests : UnitTestBase
{
    private readonly PersonEmailUpdateValidator _sut = new();

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            PersonEmailUpdateValidator.MessageEmailAddressRequired.Should().Be("Email address is required.");
            PersonEmailUpdateValidator.MessageEmailAddressInvalid.Should().Be("A valid email address is required.");
            PersonEmailUpdateValidator.MessageEmailAddressTooLong.Should().Be("Email address must not exceed 50 characters.");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validator_fails_with_Rule01_when_email_is_null_or_empty(string? email)
    {
        var model = new PersonEmailUpdateModel { EmailAddress = email! };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-01");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public async Task Validator_fails_with_Rule02_when_email_format_is_invalid(string email)
    {
        var model = new PersonEmailUpdateModel { EmailAddress = email };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Validator_passes_when_email_is_valid()
    {
        var model = new PersonEmailUpdateModel { EmailAddress = "valid@example.com" };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Fact]
    public async Task Validator_fails_with_Rule04_when_email_exceeds_50_chars()
    {
        // 44-char local part + @example.com = 56 chars total
        var model = new PersonEmailUpdateModel { EmailAddress = new string('a', 44) + "@example.com" };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-04");
    }
}
