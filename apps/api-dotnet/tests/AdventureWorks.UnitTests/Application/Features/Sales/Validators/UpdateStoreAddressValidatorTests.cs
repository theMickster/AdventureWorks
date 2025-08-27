using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

[ExcludeFromCodeCoverage]
public sealed class UpdateStoreAddressValidatorTests : UnitTestBase
{
    private readonly Mock<IAddressTypeRepository> _mockAddressTypeRepository = new();
    private readonly UpdateStoreAddressValidator _sut;

    public UpdateStoreAddressValidatorTests()
    {
        _sut = new UpdateStoreAddressValidator(_mockAddressTypeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_when_dependency_is_null()
    {
        _ = ((Action)(() => _ = new UpdateStoreAddressValidator(null!)))
            .Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Validator_fails_when_AddressTypeId_is_not_positive(int addressTypeId)
    {
        var model = new StoreAddressUpdateModel { AddressTypeId = addressTypeId };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_fails_when_AddressTypeId_does_not_existAsync()
    {
        _mockAddressTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var model = new StoreAddressUpdateModel { AddressTypeId = 999 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.AddressTypeId).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_succeeds_when_AddressTypeId_is_validAsync()
    {
        _mockAddressTypeRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new StoreAddressUpdateModel { AddressTypeId = 2 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.AddressTypeId);
    }
}
