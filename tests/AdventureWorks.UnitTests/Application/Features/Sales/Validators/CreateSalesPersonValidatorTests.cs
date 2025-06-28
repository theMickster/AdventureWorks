using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class CreateSalesPersonValidatorTests : UnitTestBase
{
    private readonly CreateSalesPersonValidator _sut = new();

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        CreateSalesPersonValidator.MessageBusinessEntityIdRequired.Should().Be("Business Entity ID must be greater than 0");
        SalesPersonBaseModelValidator<SalesPersonCreateModel>.MessageCommissionPctNonNegative.Should().Be("Commission percentage must be greater than or equal to 0");
        SalesPersonBaseModelValidator<SalesPersonCreateModel>.MessageCommissionPctMaxValue.Should().Be("Commission percentage cannot exceed 100% (1.0)");
        SalesPersonBaseModelValidator<SalesPersonCreateModel>.MessageBonusNonNegative.Should().Be("Bonus must be greater than or equal to 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_should_have_business_entity_id_errors(int businessEntityId)
    {
        var model = new SalesPersonCreateModel
        {
            BusinessEntityId = businessEntityId,
            CommissionPct = 0.05m,
            Bonus = 0
        };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BusinessEntityId);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Validator_should_have_commission_pct_errors(decimal commissionPct)
    {
        var model = new SalesPersonCreateModel
        {
            BusinessEntityId = 100,
            CommissionPct = commissionPct,
            Bonus = 0
        };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CommissionPct);
    }

    [Fact]
    public void Validator_should_have_bonus_error_when_negative()
    {
        var model = new SalesPersonCreateModel
        {
            BusinessEntityId = 100,
            CommissionPct = 0.05m,
            Bonus = -100
        };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Bonus);
    }

    [Fact]
    public void Validator_succeeds_when_all_data_is_valid()
    {
        var model = new SalesPersonCreateModel
        {
            BusinessEntityId = 100,
            CommissionPct = 0.05m,
            Bonus = 1000,
            TerritoryId = 1,
            SalesQuota = 250000
        };
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
