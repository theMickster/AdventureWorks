using AdventureWorks.Application.Features.Person.Validators;
using AdventureWorks.Models.Features.Person;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Person.Validators;

[ExcludeFromCodeCoverage]
public sealed class PersonEmailCreateValidatorTests : UnitTestBase
{
    private readonly PersonEmailCreateValidator _sut = new();

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            PersonEmailCreateValidator.MessageEmailAddressRequired.Should().Be("Email address is required.");
            PersonEmailCreateValidator.MessageEmailAddressInvalid.Should().Be("A valid email address is required.");
            PersonEmailCreateValidator.MessageEmailAddressTooLong.Should().Be("Email address must not exceed 50 characters.");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validator_fails_with_Rule01_when_email_is_null_or_empty(string? email)
    {
        var model = new PersonEmailCreateModel { EmailAddress = email! };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-01");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public async Task Validator_fails_with_Rule02_when_email_format_is_invalid(string email)
    {
        var model = new PersonEmailCreateModel { EmailAddress = email };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-02");
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("jane.doe+tag@sub.domain.org")]
    public async Task Validator_passes_when_email_is_valid(string email)
    {
        var model = new PersonEmailCreateModel { EmailAddress = email };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Fact]
    public async Task Validator_fails_with_Rule04_when_email_exceeds_50_chars()
    {
        // 44-char local part + @example.com = 56 chars total
        var model = new PersonEmailCreateModel { EmailAddress = new string('a', 44) + "@example.com" };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-04");
    }
}
