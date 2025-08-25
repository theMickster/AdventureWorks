using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

[ExcludeFromCodeCoverage]
public sealed class UpdateStoreContactValidatorTests : UnitTestBase
{
    private readonly Mock<IContactTypeEntityRepository> _mockContactTypeRepository = new();
    private readonly UpdateStoreContactValidator _sut;

    public UpdateStoreContactValidatorTests()
    {
        _sut = new UpdateStoreContactValidator(_mockContactTypeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_dependency_is_null()
    {
        _ = ((Action)(() => _ = new UpdateStoreContactValidator(null!)))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        UpdateStoreContactValidator.MessageContactTypeIdInvalid.Should().Be("The specified contact type does not exist.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Validator_fails_when_ContactTypeId_is_not_positive(int contactTypeId)
    {
        var model = new StoreContactUpdateModel { ContactTypeId = contactTypeId };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ContactTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_fails_when_ContactTypeId_does_not_existAsync()
    {
        _mockContactTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var model = new StoreContactUpdateModel { ContactTypeId = 999 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ContactTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_succeeds_when_ContactTypeId_is_validAsync()
    {
        _mockContactTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreContactUpdateModel { ContactTypeId = 11 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ContactTypeId);
    }
}
