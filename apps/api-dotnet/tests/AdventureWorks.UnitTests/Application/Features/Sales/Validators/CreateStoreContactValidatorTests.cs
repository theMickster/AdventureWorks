using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

[ExcludeFromCodeCoverage]
public sealed class CreateStoreContactValidatorTests : UnitTestBase
{
    private readonly Mock<IPersonRepository> _mockPersonRepository = new();
    private readonly Mock<IContactTypeEntityRepository> _mockContactTypeRepository = new();
    private readonly CreateStoreContactValidator _sut;

    public CreateStoreContactValidatorTests()
    {
        _sut = new CreateStoreContactValidator(_mockPersonRepository.Object, _mockContactTypeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_dependencies_are_null()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateStoreContactValidator(null!, _mockContactTypeRepository.Object)))
                .Should().Throw<ArgumentNullException>();

            _ = ((Action)(() => _ = new CreateStoreContactValidator(_mockPersonRepository.Object, null!)))
                .Should().Throw<ArgumentNullException>();
        }
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            CreateStoreContactValidator.MessagePersonIdInvalid.Should().Be("The specified person does not exist.");
            CreateStoreContactValidator.MessageContactTypeIdInvalid.Should().Be("The specified contact type does not exist.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_fails_when_PersonId_is_not_positive(int personId)
    {
        _mockContactTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreContactCreateModel { PersonId = personId, ContactTypeId = 11 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PersonId).WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Validator_fails_when_PersonId_does_not_existAsync()
    {
        _mockPersonRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockContactTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreContactCreateModel { PersonId = 999, ContactTypeId = 11 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PersonId).WithErrorCode("Rule-02");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validator_fails_when_ContactTypeId_is_not_positive(int contactTypeId)
    {
        _mockPersonRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreContactCreateModel { PersonId = 100, ContactTypeId = contactTypeId };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ContactTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_fails_when_ContactTypeId_does_not_existAsync()
    {
        _mockPersonRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockContactTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var model = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 999 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ContactTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        _mockPersonRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockContactTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreContactCreateModel { PersonId = 100, ContactTypeId = 11 };

        var result = await _sut.TestValidateAsync(model);

        using (new AssertionScope())
        {
            result.ShouldNotHaveValidationErrorFor(x => x.PersonId);
            result.ShouldNotHaveValidationErrorFor(x => x.ContactTypeId);
        }
    }
}
