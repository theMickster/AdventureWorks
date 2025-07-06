using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class HireEmployeeValidatorTests : UnitTestBase
{
    private readonly HireEmployeeValidator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_employee_id_error_when_not_positiveAsync(int employeeId)
    {
        var model = GetValidHireEmployeeModel();
        model.EmployeeId = employeeId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("EmployeeId must be greater than 0.");
    }

    [Fact]
    public async Task Validator_should_have_hire_date_error_when_emptyAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.HireDate = default;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.HireDate)
            .WithErrorMessage("HireDate is required.");
    }

    [Fact]
    public async Task Validator_should_have_hire_date_error_when_too_far_in_futureAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.HireDate = DateTime.UtcNow.AddDays(31); // More than 30 days in future

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.HireDate)
            .WithErrorMessage("HireDate cannot be more than 30 days in the future.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_should_have_department_id_error_when_not_positiveAsync(short departmentId)
    {
        var model = GetValidHireEmployeeModel();
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
        var model = GetValidHireEmployeeModel();
        model.ShiftId = shiftId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ShiftId)
            .WithErrorMessage("ShiftId must be between 1 and 3 (1=Day, 2=Evening, 3=Night).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task Validator_should_have_initial_pay_rate_error_when_not_positiveAsync(decimal initialPayRate)
    {
        var model = GetValidHireEmployeeModel();
        model.InitialPayRate = initialPayRate;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.InitialPayRate)
            .WithErrorMessage("InitialPayRate must be greater than 0.");
    }

    [Theory]
    [InlineData(500.01)]
    [InlineData(1000.00)]
    [InlineData(999999.99)]
    public async Task Validator_should_have_initial_pay_rate_error_when_exceeds_maximumAsync(decimal initialPayRate)
    {
        var model = GetValidHireEmployeeModel();
        model.InitialPayRate = initialPayRate;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.InitialPayRate)
            .WithErrorMessage("InitialPayRate cannot exceed $500.00.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(255)]
    public async Task Validator_should_have_pay_frequency_error_when_invalidAsync(byte payFrequency)
    {
        var model = GetValidHireEmployeeModel();
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
        var model = GetValidHireEmployeeModel();
        model.ManagerId = managerId;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("ManagerId must be greater than 0 when provided.");
    }

    [Fact]
    public async Task Validator_should_not_have_manager_id_error_when_nullAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.ManagerId = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ManagerId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_initial_vacation_hours_error_when_negativeAsync(short vacationHours)
    {
        var model = GetValidHireEmployeeModel();
        model.InitialVacationHours = vacationHours;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.InitialVacationHours)
            .WithErrorMessage("InitialVacationHours must be greater than or equal to 0.");
    }

    [Theory]
    [InlineData(241)]
    [InlineData(500)]
    [InlineData(9999)]
    public async Task Validator_should_have_initial_vacation_hours_error_when_exceeds_maximumAsync(short vacationHours)
    {
        var model = GetValidHireEmployeeModel();
        model.InitialVacationHours = vacationHours;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.InitialVacationHours)
            .WithErrorMessage("InitialVacationHours cannot exceed 240 hours.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validator_should_have_initial_sick_leave_hours_error_when_negativeAsync(short sickLeaveHours)
    {
        var model = GetValidHireEmployeeModel();
        model.InitialSickLeaveHours = sickLeaveHours;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.InitialSickLeaveHours)
            .WithErrorMessage("InitialSickLeaveHours must be greater than or equal to 0.");
    }

    [Theory]
    [InlineData(481)]
    [InlineData(1000)]
    [InlineData(9999)]
    public async Task Validator_should_have_initial_sick_leave_hours_error_when_exceeds_maximumAsync(short sickLeaveHours)
    {
        var model = GetValidHireEmployeeModel();
        model.InitialSickLeaveHours = sickLeaveHours;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.InitialSickLeaveHours)
            .WithErrorMessage("InitialSickLeaveHours cannot exceed 480 hours.");
    }

    [Fact]
    public async Task Validator_should_have_notes_error_when_exceeds_maximum_lengthAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.Notes = new string('a', 501); // 501 characters

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 500 characters.");
    }

    [Fact]
    public async Task Validator_should_not_have_notes_error_when_nullAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.Notes = null;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public async Task Validator_succeeds_when_all_data_is_validAsync()
    {
        var model = GetValidHireEmployeeModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_succeeds_when_hire_date_is_todayAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.HireDate = DateTime.UtcNow;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.HireDate);
    }

    [Fact]
    public async Task Validator_succeeds_when_hire_date_is_30_days_in_futureAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.HireDate = DateTime.UtcNow.Date.AddDays(30);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.HireDate);
    }

    [Fact]
    public async Task Validator_succeeds_when_initial_pay_rate_is_at_maximumAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.InitialPayRate = 500.00m;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.InitialPayRate);
    }

    [Fact]
    public async Task Validator_succeeds_when_optional_fields_are_at_boundariesAsync()
    {
        var model = GetValidHireEmployeeModel();
        model.InitialVacationHours = 240; // Max
        model.InitialSickLeaveHours = 480; // Max
        model.Notes = new string('a', 500); // Max length

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static EmployeeHireModel GetValidHireEmployeeModel()
    {
        return new EmployeeHireModel
        {
            EmployeeId = 1,
            HireDate = DateTime.UtcNow,
            DepartmentId = 1,
            ShiftId = 1,
            InitialPayRate = 45.00m,
            PayFrequency = 2,
            ManagerId = 100,
            InitialVacationHours = 40,
            InitialSickLeaveHours = 24,
            Notes = "Standard new hire"
        };
    }
}
