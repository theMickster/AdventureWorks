using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Validators.Address;

namespace AdventureWorks.UnitTests.Application.Validators.Address;

[ExcludeFromCodeCoverage]
public sealed class UpdateAddressValidatorTests : UnitTestBase
{
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private readonly UpdateAddressValidator _sut;

    public UpdateAddressValidatorTests()
    {
        _sut = new UpdateAddressValidator(_mockStateProvinceRepository.Object);
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        UpdateAddressValidator.MessageAddressLine1Empty.Should().Be("Address Line 1 cannot be null, empty, or whitespace");
        UpdateAddressValidator.MessageAddressLine1Length.Should().Be("Address Line 1 cannot be greater than 60 characters");
        UpdateAddressValidator.MessageAddressLine2Length.Should().Be("Address Line 2 cannot be greater than 60 characters");
        UpdateAddressValidator.MessageCityEmpty.Should().Be("City cannot be null, empty, or whitespace");
        UpdateAddressValidator.MessageCityLength.Should().Be("City cannot be greater than 30 characters");
        UpdateAddressValidator.PostalCodeEmpty.Should().Be("Postal Code cannot be null, empty, or whitespace");
        UpdateAddressValidator.PostalCodeLength.Should().Be("Postal Code cannot be greater than 15 characters");
        UpdateAddressValidator.StateProvinceIdExists.Should().Be("StateProvince Id must exist prior to use");
        UpdateAddressValidator.StateProvinceExists.Should().Be("StateProvince is required");

        UpdateAddressValidator.AddressIdValidInteger.Should().Be("Address Id must be a positive integer.");
    }
}