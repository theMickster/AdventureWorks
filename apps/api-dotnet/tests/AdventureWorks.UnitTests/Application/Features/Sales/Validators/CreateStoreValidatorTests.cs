using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class CreateStoreValidatorTests : UnitTestBase
{
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private readonly CreateStoreValidator _sut;

    public CreateStoreValidatorTests()
    {
        _sut = new CreateStoreValidator(_mockSalesPersonRepository.Object);
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        CreateStoreValidator.MessageStoreNameEmpty.Should().Be("Store name cannot be null, empty, or whitespace");
        CreateStoreValidator.MessageStoreNameLength.Should().Be("Store name cannot be greater than 50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validator_should_have_store_name_errors(string name)
    {
        var model = new StoreCreateModel { Name = name };
        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validator_fails_when_store_name_is_too_long()
    {
        var model = new StoreCreateModel { Name = new string('a', 51) };
        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validator_succeeds_when_store_name_is_exactly_50_characters()
    {
        var model = new StoreCreateModel { Name = new string('a', 50) };
        var result = await _sut.TestValidateAsync(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_valid()
    {
        var model = new StoreCreateModel { Name = "Valid Store Name" };
        var result = await _sut.TestValidateAsync(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task ValidateAsync_Fails_When_SalesPersonId_DoesNotExist()
    {
        _mockSalesPersonRepository
            .Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var model = new StoreCreateModel { Name = "Valid Store Name", SalesPersonId = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.SalesPersonId)
              .WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task ValidateAsync_Succeeds_When_SalesPersonId_IsValid()
    {
        _mockSalesPersonRepository
            .Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var model = new StoreCreateModel { Name = "Valid Store Name", SalesPersonId = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.SalesPersonId);
    }

    [Fact]
    public async Task ValidateAsync_Succeeds_When_SalesPersonId_IsNull()
    {
        var model = new StoreCreateModel { Name = "Valid Store Name", SalesPersonId = null };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.SalesPersonId);
    }
}
