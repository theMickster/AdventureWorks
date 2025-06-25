using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class CreateStoreValidatorTests : UnitTestBase
{
    private readonly CreateStoreValidator _sut = new();

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        CreateStoreValidator.MessageStoreNameEmpty.Should().Be("Store name cannot be null, empty, or whitespace");
        CreateStoreValidator.MessageStoreNameLength.Should().Be("Store name cannot be greater than 50 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_should_have_store_name_errors(string name)
    {
        var model = new StoreCreateModel { Name = name };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_fails_when_store_name_is_too_long()
    {
        var model = new StoreCreateModel { Name = new string('a', 61) };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_succeeds_when_all_data_is_valid()
    {
        var model = new StoreCreateModel { Name = "Valid Store Name" };
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
