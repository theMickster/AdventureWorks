using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Models.Slim;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class CreateSalesPersonValidatorTests : UnitTestBase
{
    private readonly Mock<IPhoneNumberTypeRepository> _mockPhoneNumberTypeRepository = new();
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private readonly Mock<IAddressTypeRepository> _mockAddressTypeRepository = new();
    private readonly CreateSalesPersonValidator _sut;

    public CreateSalesPersonValidatorTests()
    {
        _sut = new CreateSalesPersonValidator(
            _mockPhoneNumberTypeRepository.Object,
            _mockStateProvinceRepository.Object,
            _mockAddressTypeRepository.Object);

        // Setup default mocks to return valid entities
        _mockPhoneNumberTypeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell" });

        _mockStateProvinceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new StateProvinceEntity { StateProvinceId = 79, Name = "Washington" });

        _mockAddressTypeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AddressTypeEntity { AddressTypeId = 2, Name = "Work" });
    }

    private static SalesPersonCreateModel GetValidModel()
    {
        return new SalesPersonCreateModel
        {
            FirstName = "Jane",
            LastName = "Smith",
            NationalIdNumber = "987654321",
            LoginId = "adventure-works\\jane.smith",
            JobTitle = "Sales Rep",
            BirthDate = new DateTime(1988, 8, 20),
            HireDate = new DateTime(2019, 3, 15),
            MaritalStatus = "M",
            Gender = "F",
            Phone = new SalesPersonPhoneCreateModel
            {
                PhoneNumber = "555-987-6543",
                PhoneNumberTypeId = 1
            },
            EmailAddress = "jane.smith@adventure-works.com",
            Address = new AddressCreateModel
            {
                AddressLine1 = "789 Sales Avenue",
                City = "Seattle",
                PostalCode = "98102",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
            },
            AddressTypeId = 2,
            TerritoryId = 1,
            SalesQuota = 300000,
            Bonus = 5000,
            CommissionPct = 0.02m
        };
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
       
        SalesPersonBaseModelValidator<SalesPersonCreateModel>.MessageCommissionPctNonNegative.Should().Be("Commission percentage must be greater than or equal to 0");
        SalesPersonBaseModelValidator<SalesPersonCreateModel>.MessageCommissionPctMaxValue.Should().Be("Commission percentage cannot exceed 100% (1.0)");
        SalesPersonBaseModelValidator<SalesPersonCreateModel>.MessageBonusNonNegative.Should().Be("Bonus must be greater than or equal to 0");
    }
    
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public async Task Validator_should_have_commission_pct_errorsAsync(decimal commissionPct)
    {
        var model = GetValidModel();
        model.CommissionPct = commissionPct;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.CommissionPct);
    }

    [Fact]
    public async Task Validator_should_have_bonus_error_when_negativeAsync()
    {
        var model = GetValidModel();
        model.Bonus = -100;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Bonus);
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var model = GetValidModel();

        var result = await _sut.TestValidateAsync(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
