using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class UpdateSalesPersonSalesConfigValidatorTests : UnitTestBase
{
    private readonly Mock<ISalesTerritoryRepository> _mockTerritoryRepository = new();
    private readonly UpdateSalesPersonSalesConfigValidator _sut;

    public UpdateSalesPersonSalesConfigValidatorTests()
    {
        _mockTerritoryRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesTerritoryEntity { TerritoryId = 2, Name = "Central" });

        _sut = new UpdateSalesPersonSalesConfigValidator(_mockTerritoryRepository.Object);
    }

    private static SalesPersonSalesConfigUpdateModel GetValidModel()
    {
        return new SalesPersonSalesConfigUpdateModel
        {
            Id = 100,
            TerritoryId = 2,
            SalesQuota = 300000,
            Bonus = 2000,
            CommissionPct = 0.12m
        };
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        UpdateSalesPersonSalesConfigValidator.MessageIdRequired.Should().Be("Sales Person ID must be greater than 0");
        UpdateSalesPersonSalesConfigValidator.MessageTerritoryIdMustExist.Should().Be("Territory ID must reference an existing sales territory");
        SalesPersonBaseModelValidator<SalesPersonSalesConfigUpdateModel>.MessageCommissionPctNonNegative.Should().Be("Commission percentage must be greater than or equal to 0");
        SalesPersonBaseModelValidator<SalesPersonSalesConfigUpdateModel>.MessageCommissionPctMaxValue.Should().Be("Commission percentage cannot exceed 100% (1.0)");
        SalesPersonBaseModelValidator<SalesPersonSalesConfigUpdateModel>.MessageBonusNonNegative.Should().Be("Bonus must be greater than or equal to 0");
        SalesPersonBaseModelValidator<SalesPersonSalesConfigUpdateModel>.MessageSalesQuotaPositive.Should().Be("Sales quota must be greater than 0 when specified");
        SalesPersonBaseModelValidator<SalesPersonSalesConfigUpdateModel>.MessageTerritoryIdPositive.Should().Be("Territory ID must be greater than 0 when specified");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_id_error(int id)
    {
        var model = GetValidModel();
        model.Id = id;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public async Task Validator_should_have_commission_pct_errors(decimal commissionPct)
    {
        var model = GetValidModel();
        model.CommissionPct = commissionPct;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.CommissionPct);
    }

    [Fact]
    public async Task Validator_should_have_bonus_error_when_negative()
    {
        var model = GetValidModel();
        model.Bonus = -100;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Bonus);
    }

    [Fact]
    public async Task Validator_should_have_sales_quota_error_when_zero()
    {
        var model = GetValidModel();
        model.SalesQuota = 0;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.SalesQuota);
    }

    [Fact]
    public async Task Validator_should_have_territory_id_error_when_zero()
    {
        var model = GetValidModel();
        model.TerritoryId = 0;

        // Rule-05 (base: > 0 when set) fires; Rule-07 (exists) is skipped because TerritoryId == 0
        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.TerritoryId);
    }

    [Fact]
    public async Task Validator_should_have_territory_exists_error_when_territory_not_found()
    {
        _mockTerritoryRepository
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesTerritoryEntity?)null);

        var model = GetValidModel();
        model.TerritoryId = 999;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.TerritoryId)
            .WithErrorCode("Rule-07");
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_valid()
    {
        var model = GetValidModel();

        var result = await _sut.TestValidateAsync(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_succeeds_when_optional_fields_are_null()
    {
        var model = GetValidModel();
        model.TerritoryId = null;
        model.SalesQuota = null;

        // Async rule is skipped when TerritoryId is null; no repository call made
        var result = await _sut.TestValidateAsync(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
