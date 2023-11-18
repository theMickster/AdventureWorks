using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Validators.Address;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AdventureWorks.Domain.Models.Slim;
using AdventureWorks.Test.Common.Utilities;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Validators.Address;

[ExcludeFromCodeCoverage]
public sealed class CreateAddressValidatorTests : UnitTestBase
{
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private readonly CreateAddressValidator _sut;

    public CreateAddressValidatorTests()
    {
        _sut = new CreateAddressValidator(_mockStateProvinceRepository.Object);
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        CreateAddressValidator.MessageAddressLine1Empty.Should().Be("Address Line 1 cannot be null, empty, or whitespace");
        CreateAddressValidator.MessageAddressLine1Length.Should().Be("Address Line 1 cannot be greater than 60 characters");
        CreateAddressValidator.MessageAddressLine2Length.Should().Be("Address Line 2 cannot be greater than 60 characters");
        CreateAddressValidator.MessageCityEmpty.Should().Be("City cannot be null, empty, or whitespace");
        CreateAddressValidator.MessageCityLength.Should().Be("City cannot be greater than 30 characters");
        CreateAddressValidator.PostalCodeEmpty.Should().Be("Postal Code cannot be null, empty, or whitespace");
        CreateAddressValidator.PostalCodeLength.Should().Be("Postal Code cannot be greater than 15 characters");
        CreateAddressValidator.StateProvinceIdExists.Should().Be("AddressStateProvince Id must exist prior to use");
        CreateAddressValidator.StateProvinceExists.Should().Be("AddressStateProvince is required");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        const int stateId = 7;

        _mockStateProvinceRepository.Setup(x => x.GetByIdAsync(stateId))
            .ReturnsAsync(new StateProvinceEntity { StateProvinceId = stateId, Name = "Colorado" });

        var validAddress = new AddressCreateModel
        {
            AddressLine1 = StringGenerator.GetRandomString(60),
            AddressLine2 = StringGenerator.GetRandomString(60),
            City = StringGenerator.GetRandomString(30),
            AddressStateProvince = new GenericSlimModel { Id = stateId },
            PostalCode = StringGenerator.GetRandomString(15)
        };

        var validationResult = await _sut.TestValidateAsync(validAddress);

        using (new AssertionScope())
        {
            validationResult.ShouldNotHaveAnyValidationErrors();
            validationResult.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Validator_when_address_is_invalidAsync()
    {
        _mockStateProvinceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))!
            .ReturnsAsync((StateProvinceEntity)null!);

        using (new AssertionScope())
        {
            var validationResult = await _sut.TestValidateAsync(new AddressCreateModel());

            validationResult.ShouldHaveValidationErrorFor(a => a.AddressLine1)
                .WithErrorCode("Rule-01");

            validationResult = await _sut.TestValidateAsync(
                    new AddressCreateModel
                    {
                        AddressLine1 = string.Empty
                    })
                ;

            validationResult.ShouldHaveValidationErrorFor(a => a.AddressLine1)
                .WithErrorCode("Rule-01");

            validationResult = await _sut.TestValidateAsync(
                    new AddressCreateModel
                    {
                        AddressLine1 = StringGenerator.GetRandomString(61)
                    })
                ;

            validationResult.ShouldHaveValidationErrorFor(a => a.AddressLine1)
                .WithErrorCode("Rule-02");

            validationResult.ShouldHaveValidationErrorFor(a => a.AddressStateProvince)
                .WithErrorCode("Rule-08");
        }
    }


    [Fact]
    public async Task Validator_when_address_state_does_not_exists_is_invalidAsync()
    {
        _mockStateProvinceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))!
            .ReturnsAsync((StateProvinceEntity)null!);

        using (new AssertionScope())
        {
            var validationResult = await _sut.TestValidateAsync(new AddressCreateModel
                {
                AddressStateProvince = new GenericSlimModel
                {
                    Id = 1548,
                    Code = "ABCDEFG"
                }
            })
                ;

            validationResult.ShouldHaveValidationErrorFor(a => a.AddressStateProvince)
                .WithErrorCode("Rule-07");
        }
    }
}