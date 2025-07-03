using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

public sealed class UpdateSalesPersonValidatorTests : UnitTestBase
{
    private readonly UpdateSalesPersonValidator _sut = new();

    private static SalesPersonUpdateModel GetValidUpdateModel()
    {
        return new SalesPersonUpdateModel
        {
            Id = 100,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = "A.",
            Title = "Ms.",
            Suffix = "Jr.",
            JobTitle = "Senior Sales Rep",
            MaritalStatus = "M",
            Gender = "F",
            SalariedFlag = true,
            OrganizationLevel = 2,
            TerritoryId = 2,
            SalesQuota = 300000,
            Bonus = 2000,
            CommissionPct = 0.06m
        };
    }

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        UpdateSalesPersonValidator.MessageIdRequired.Should().Be("Sales Person ID must be greater than 0");
        UpdateSalesPersonValidator.MessageFirstNameLength.Should().Be("First name cannot be greater than 50 characters");
        UpdateSalesPersonValidator.MessageLastNameLength.Should().Be("Last name cannot be greater than 50 characters");
        UpdateSalesPersonValidator.MessageJobTitleLength.Should().Be("Job title cannot be greater than 50 characters");
        UpdateSalesPersonValidator.MessageMaritalStatusInvalid.Should().Be("Marital status must be 'M' (Married) or 'S' (Single)");
        UpdateSalesPersonValidator.MessageGenderInvalid.Should().Be("Gender must be 'M' (Male) or 'F' (Female)");
        SalesPersonBaseModelValidator<SalesPersonUpdateModel>.MessageCommissionPctNonNegative.Should().Be("Commission percentage must be greater than or equal to 0");
        SalesPersonBaseModelValidator<SalesPersonUpdateModel>.MessageCommissionPctMaxValue.Should().Be("Commission percentage cannot exceed 100% (1.0)");
        SalesPersonBaseModelValidator<SalesPersonUpdateModel>.MessageBonusNonNegative.Should().Be("Bonus must be greater than or equal to 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_should_have_id_errors(int id)
    {
        var model = GetValidUpdateModel();
        model.Id = id;

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Validator_should_have_commission_pct_errors(decimal commissionPct)
    {
        var model = GetValidUpdateModel();
        model.CommissionPct = commissionPct;

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CommissionPct);
    }

    [Fact]
    public void Validator_should_have_bonus_error_when_negative()
    {
        var model = GetValidUpdateModel();
        model.Bonus = -100;

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Bonus);
    }

    [Fact]
    public void Validator_should_have_first_name_error_when_too_long()
    {
        var model = GetValidUpdateModel();
        model.FirstName = new string('A', 51);

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validator_should_have_last_name_error_when_too_long()
    {
        var model = GetValidUpdateModel();
        model.LastName = new string('B', 51);

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validator_should_have_job_title_error_when_too_long()
    {
        var model = GetValidUpdateModel();
        model.JobTitle = new string('C', 51);

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.JobTitle);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("A")]
    [InlineData("")]
    public void Validator_should_have_marital_status_error_when_invalid(string maritalStatus)
    {
        var model = GetValidUpdateModel();
        model.MaritalStatus = maritalStatus;

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("A")]
    [InlineData("")]
    public void Validator_should_have_gender_error_when_invalid(string gender)
    {
        var model = GetValidUpdateModel();
        model.Gender = gender;

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Validator_should_have_organization_level_error_when_negative()
    {
        var model = GetValidUpdateModel();
        model.OrganizationLevel = -1;

        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.OrganizationLevel);
    }

    [Fact]
    public void Validator_succeeds_when_all_data_is_valid()
    {
        var model = GetValidUpdateModel();

        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
