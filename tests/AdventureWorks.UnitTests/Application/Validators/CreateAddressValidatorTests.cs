using AdventureWorks.Application.Validators;
using AdventureWorks.Domain.Models;
using AdventureWorks.Test.Common.Utilities;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Validators
{
    [ExcludeFromCodeCoverage]
    public sealed class CreateAddressValidatorTests
    {
        private readonly CreateAddressValidator _sut = new();

        [Fact]
        public void Validator_error_messages_are_correct()
        {
            CreateAddressValidator.MessageAddressLine1Empty.Should().Be("Address Line 1 cannot be null, empty, or whitespace");
            CreateAddressValidator.MessageAddressLine1Length.Should().Be("Address Line 1 cannot be greater than 60 characters");
            CreateAddressValidator.MessageAddressLine2Length.Should().Be("Address Line 2 cannot be greater than 60 characters");
            CreateAddressValidator.MessageCityEmpty.Should().Be("City cannot be null, empty, or whitespace");
            CreateAddressValidator.MessageCityLength.Should().Be("City cannot be greater than 30 characters");
            CreateAddressValidator.PostalCodeEmpty.Should().Be("Postal Code cannot be null, empty, or whitespace");
            CreateAddressValidator.PostalCodeEmptyLength.Should().Be("Postal Code cannot be null, empty, or whitespace");
        }

        [Fact]
        public async Task Validator_succeeds_when_all_data_is_validAsync()
        {
            var validAddress = new AddressCreateModel()
            {
                AddressLine1 = StringGenerator.GetRandomString(60),
                AddressLine2 = StringGenerator.GetRandomString(60),
                City = StringGenerator.GetRandomString(30),
                StateProvince = new StateProvinceModel{ Id = 7 },
                PostalCode = StringGenerator.GetRandomString(15)
            };

            var validationResult = await _sut.TestValidateAsync(validAddress).ConfigureAwait(false);

            using (new AssertionScope())
            {
                validationResult.ShouldNotHaveAnyValidationErrors();
                validationResult.IsValid.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Validator_when_address_is_invalidAsync()
        {
            using (new AssertionScope())
            {
                var validationResult = await _sut.TestValidateAsync(new AddressCreateModel()).ConfigureAwait(false);

                validationResult.ShouldHaveValidationErrorFor(a => a.AddressLine1)
                    .WithErrorCode("Rule-01");

                validationResult = await _sut.TestValidateAsync(
                        new AddressCreateModel
                        {
                            AddressLine1 = string.Empty
                        })
                    .ConfigureAwait(false);

                validationResult.ShouldHaveValidationErrorFor(a => a.AddressLine1)
                    .WithErrorCode("Rule-01");

                validationResult = await _sut.TestValidateAsync(
                        new AddressCreateModel
                        {
                            AddressLine1 = StringGenerator.GetRandomString(61)
                        })
                    .ConfigureAwait(false);

                validationResult.ShouldHaveValidationErrorFor(a => a.AddressLine1)
                    .WithErrorCode("Rule-02");
            }
        }


    }
}
