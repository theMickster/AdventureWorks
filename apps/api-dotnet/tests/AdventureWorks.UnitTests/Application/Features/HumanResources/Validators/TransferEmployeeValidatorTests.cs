using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class TransferEmployeeValidatorTests : UnitTestBase
{
    private readonly TransferEmployeeValidator _sut = new();

    [Fact]
    public async Task DepartmentId_of_zero_returns_validation_errorAsync()
    {
        var model = GetValidTransferModel();
        model.DepartmentId = 0;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public async Task ShiftId_of_zero_returns_rule_02_errorAsync()
    {
        var model = GetValidTransferModel();
        model.ShiftId = 0;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ShiftId)
            .WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task ShiftId_of_four_returns_rule_02_errorAsync()
    {
        var model = GetValidTransferModel();
        model.ShiftId = 4;

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.ShiftId)
            .WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Valid_model_passes_validationAsync()
    {
        var model = GetValidTransferModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static EmployeeTransferModel GetValidTransferModel()
    {
        return new EmployeeTransferModel
        {
            DepartmentId = 1,
            ShiftId = 1
        };
    }
}
