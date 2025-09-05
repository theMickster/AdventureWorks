using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class RecordPayChangeValidatorTests : UnitTestBase
{
    private readonly RecordPayChangeValidator _sut = new();

    [Fact]
    public async Task Rate_zero_fails_Rule01Async()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 0m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Rate)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Rate_negative_fails_Rule01Async()
    {
        var model = new EmployeePayChangeCreateModel { Rate = -1m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Rate)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Rate_below_minimum_fails_Rule01Async()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 6.49m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Rate)
            .WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Rate_at_minimum_passesAsync()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 6.50m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Rate);
    }

    [Fact]
    public async Task Rate_exceeds_maximum_fails_Rule02Async()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 200.01m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Rate)
            .WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Rate_at_maximum_passesAsync()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 200m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Rate);
    }

    [Fact]
    public async Task Rate_within_bounds_passesAsync()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Rate);
    }

    [Fact]
    public async Task PayFrequency_zero_fails_Rule03Async()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 0 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PayFrequency)
            .WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task PayFrequency_three_fails_Rule03Async()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 3 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PayFrequency)
            .WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task PayFrequency_monthly_passesAsync()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 1 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.PayFrequency);
    }

    [Fact]
    public async Task PayFrequency_biweekly_passesAsync()
    {
        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 2 };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.PayFrequency);
    }
}
