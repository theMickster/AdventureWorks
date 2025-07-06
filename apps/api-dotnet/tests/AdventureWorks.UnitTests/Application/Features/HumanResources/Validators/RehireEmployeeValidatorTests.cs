using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class RehireEmployeeValidatorTests : UnitTestBase
{
    private readonly RehireEmployeeValidator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_employee_id_error_when_not_positiveAsync(int employeeId)
    {
        var model = GetValidRehireEmployeeModel();
        model.EmployeeId = employeeId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("EmployeeId must be greater than 0.");
    }

    [Fact]
    public async Task Validator_should_have_rehire_date_error_when_emptyAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.RehireDate = default;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.RehireDate)
            .WithErrorMessage("RehireDate is required.");
    }

    [Fact]
    public async Task Validator_should_have_rehire_date_error_when_in_pastAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.RehireDate = DateTime.UtcNow.Date.AddDays(-1); // Yesterday

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.RehireDate)
            .WithErrorMessage("RehireDate cannot be in the past.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_department_id_error_when_not_positiveAsync(short departmentId)
    {
        var model = GetValidRehireEmployeeModel();
        model.DepartmentId = departmentId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
            .WithErrorMessage("DepartmentId must be greater than 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(255)]
    public async Task Validator_should_have_shift_id_error_when_out_of_rangeAsync(byte shiftId)
    {
        var model = GetValidRehireEmployeeModel();
        model.ShiftId = shiftId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ShiftId)
            .WithErrorMessage("ShiftId must be between 1 and 3 (1=Day, 2=Evening, 3=Night).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task Validator_should_have_pay_rate_error_when_not_positiveAsync(decimal payRate)
    {
        var model = GetValidRehireEmployeeModel();
        model.PayRate = payRate;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PayRate)
            .WithErrorMessage("PayRate must be greater than 0.");
    }

    [Theory]
    [InlineData(500.01)]
    [InlineData(1000.00)]
    [InlineData(999999.99)]
    public async Task Validator_should_have_pay_rate_error_when_exceeds_maximumAsync(decimal payRate)
    {
        var model = GetValidRehireEmployeeModel();
        model.PayRate = payRate;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PayRate)
            .WithErrorMessage("PayRate cannot exceed $500.00.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(255)]
    public async Task Validator_should_have_pay_frequency_error_when_invalidAsync(byte payFrequency)
    {
        var model = GetValidRehireEmployeeModel();
        model.PayFrequency = payFrequency;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.PayFrequency)
            .WithErrorMessage("PayFrequency must be 1 (Monthly) or 2 (Bi-weekly).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_manager_id_error_when_providedAndNotPositiveAsync(int managerId)
    {
        var model = GetValidRehireEmployeeModel();
        model.ManagerId = managerId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("ManagerId must be greater than 0 when provided.");
    }

    [Fact]
    public async Task Validator_should_not_have_manager_id_error_when_nullAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.ManagerId = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ManagerId);
    }

    [Fact]
    public async Task Validator_should_have_notes_error_when_exceeds_maximum_lengthAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.Notes = new string('a', 501); // 501 characters

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 500 characters.");
    }

    [Fact]
    public async Task Validator_should_not_have_notes_error_when_nullAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.Notes = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var model = GetValidRehireEmployeeModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_succeeds_when_rehire_date_is_todayAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.RehireDate = DateTime.UtcNow.Date;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RehireDate);
    }

    [Fact]
    public async Task Validator_succeeds_when_rehire_date_is_in_futureAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.RehireDate = DateTime.UtcNow.Date.AddDays(30);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.RehireDate);
    }

    [Fact]
    public async Task Validator_succeeds_when_pay_rate_is_at_maximumAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.PayRate = 500.00m;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.PayRate);
    }

    [Fact]
    public async Task Validator_succeeds_when_notes_is_at_maximum_lengthAsync()
    {
        var model = GetValidRehireEmployeeModel();
        model.Notes = new string('a', 500);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validator_succeeds_with_any_restore_seniority_valueAsync(bool restoreSeniority)
    {
        var model = GetValidRehireEmployeeModel();
        model.RestoreSeniority = restoreSeniority;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Validator_succeeds_with_valid_shift_idsAsync(byte shiftId)
    {
        var model = GetValidRehireEmployeeModel();
        model.ShiftId = shiftId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ShiftId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Validator_succeeds_with_valid_pay_frequenciesAsync(byte payFrequency)
    {
        var model = GetValidRehireEmployeeModel();
        model.PayFrequency = payFrequency;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.PayFrequency);
    }

    private static EmployeeRehireModel GetValidRehireEmployeeModel()
    {
        return new EmployeeRehireModel
        {
            EmployeeId = 1,
            RehireDate = DateTime.UtcNow.Date.AddDays(7),
            DepartmentId = 1,
            ShiftId = 1,
            PayRate = 50.00m,
            PayFrequency = 2,
            ManagerId = 100,
            RestoreSeniority = false,
            Notes = "Boomerang employee - rehire after 6 months"
        };
    }
}
