using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

[ExcludeFromCodeCoverage]
public sealed class CreateStoreAddressValidatorTests : UnitTestBase
{
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IAddressTypeRepository> _mockAddressTypeRepository = new();
    private readonly CreateStoreAddressValidator _sut;

    public CreateStoreAddressValidatorTests()
    {
        _sut = new CreateStoreAddressValidator(_mockAddressRepository.Object, _mockAddressTypeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_dependencies_are_null()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateStoreAddressValidator(null!, _mockAddressTypeRepository.Object)))
                .Should().Throw<ArgumentNullException>();

            _ = ((Action)(() => _ = new CreateStoreAddressValidator(_mockAddressRepository.Object, null!)))
                .Should().Throw<ArgumentNullException>();
        }
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            CreateStoreAddressValidator.MessageAddressTypeIdInvalid.Should().Be("The specified address type does not exist.");
            CreateStoreAddressValidator.MessageAddressIdInvalid.Should().Be("The specified address does not exist.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Validator_fails_when_AddressTypeId_is_not_positive(int addressTypeId)
    {
        _mockAddressRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = addressTypeId };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_fails_when_AddressTypeId_does_not_existAsync()
    {
        _mockAddressRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockAddressTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 999 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressTypeId).WithErrorCode("Rule-01");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_fails_when_AddressId_is_not_positive(int addressId)
    {
        _mockAddressTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreAddressCreateModel { AddressId = addressId, AddressTypeId = 2 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressId).WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Validator_fails_when_AddressId_does_not_existAsync()
    {
        _mockAddressRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockAddressTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreAddressCreateModel { AddressId = 999, AddressTypeId = 2 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressId).WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        _mockAddressRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockAddressTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 };

        var result = await _sut.TestValidateAsync(model);

        using (new AssertionScope())
        {
            result.ShouldNotHaveValidationErrorFor(x => x.AddressId);
            result.ShouldNotHaveValidationErrorFor(x => x.AddressTypeId);
        }
    }
}
