using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class TerminateEmployeeValidatorTests : UnitTestBase
{
    private readonly TerminateEmployeeValidator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_employee_id_error_when_not_positiveAsync(int employeeId)
    {
        var model = GetValidTerminateEmployeeModel();
        model.EmployeeId = employeeId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("EmployeeId must be greater than 0.");
    }

    [Fact]
    public async Task Validator_should_have_termination_date_error_when_emptyAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationDate = default;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.TerminationDate)
            .WithErrorMessage("TerminationDate is required.");
    }

    [Fact]
    public async Task Validator_should_have_termination_date_error_when_too_far_in_futureAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationDate = DateTime.UtcNow.AddDays(91); // More than 90 days in future

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.TerminationDate)
            .WithErrorMessage("TerminationDate cannot be more than 90 days in the future.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validator_should_have_reason_error_when_emptyAsync(string reason)
    {
        var model = GetValidTerminateEmployeeModel();
        model.Reason = reason!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Termination reason is required.");
    }

    [Fact]
    public async Task Validator_should_have_reason_error_when_exceeds_maximum_lengthAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.Reason = new string('a', 501); // 501 characters

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 500 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validator_should_have_termination_type_error_when_emptyAsync(string terminationType)
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationType = terminationType!;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.TerminationType)
            .WithErrorMessage("TerminationType is required.");
    }

    [Theory]
    [InlineData("Fired")]
    [InlineData("Quit")]
    [InlineData("Terminated")]
    [InlineData("InvalidType")]
    [InlineData("voluntary")] // Case sensitive
    public async Task Validator_should_have_termination_type_error_when_invalidAsync(string terminationType)
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationType = terminationType;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.TerminationType)
            .WithErrorMessage("TerminationType must be one of: Voluntary, Involuntary, Retirement, Layoff.");
    }

    [Theory]
    [InlineData("Voluntary")]
    [InlineData("Involuntary")]
    [InlineData("Retirement")]
    [InlineData("Layoff")]
    public async Task Validator_should_not_have_termination_type_error_when_validAsync(string terminationType)
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationType = terminationType;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.TerminationType);
    }

    [Fact]
    public async Task Validator_should_have_notes_error_when_exceeds_maximum_lengthAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.Notes = new string('a', 1001); // 1001 characters

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 1000 characters.");
    }

    [Fact]
    public async Task Validator_should_not_have_notes_error_when_nullAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.Notes = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var model = GetValidTerminateEmployeeModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_succeeds_when_termination_date_is_todayAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationDate = DateTime.UtcNow;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.TerminationDate);
    }

    [Fact]
    public async Task Validator_succeeds_when_termination_date_is_90_days_in_futureAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.TerminationDate = DateTime.UtcNow.Date.AddDays(90);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.TerminationDate);
    }

    [Fact]
    public async Task Validator_succeeds_when_reason_is_at_maximum_lengthAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.Reason = new string('a', 500);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task Validator_succeeds_when_notes_is_at_maximum_lengthAsync()
    {
        var model = GetValidTerminateEmployeeModel();
        model.Notes = new string('a', 1000);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validator_succeeds_with_any_eligible_for_rehire_valueAsync(bool eligibleForRehire)
    {
        var model = GetValidTerminateEmployeeModel();
        model.EligibleForRehire = eligibleForRehire;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validator_succeeds_with_any_payout_pto_valueAsync(bool payoutPto)
    {
        var model = GetValidTerminateEmployeeModel();
        model.PayoutPto = payoutPto;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static EmployeeTerminateModel GetValidTerminateEmployeeModel()
    {
        return new EmployeeTerminateModel
        {
            EmployeeId = 1,
            TerminationDate = DateTime.UtcNow,
            Reason = "Position eliminated due to restructuring",
            TerminationType = "Involuntary",
            EligibleForRehire = true,
            PayoutPto = true,
            Notes = "Exit interview completed"
        };
    }
}
