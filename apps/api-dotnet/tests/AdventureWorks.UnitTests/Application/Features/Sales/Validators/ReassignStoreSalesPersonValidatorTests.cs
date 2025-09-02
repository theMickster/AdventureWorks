using AdventureWorks.Application.Features.Sales.Validators;
using AdventureWorks.Models.Features.Sales;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Validators;

[ExcludeFromCodeCoverage]
public sealed class ReassignStoreSalesPersonValidatorTests : UnitTestBase
{
    private readonly ReassignStoreSalesPersonValidator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_fails_with_Rule01_when_SalesPersonId_is_not_positive(int salesPersonId)
    {
        var model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = salesPersonId };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(x => x.SalesPersonId).WithErrorCode("Rule-01");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    public async Task Validator_passes_when_SalesPersonId_is_positive(int salesPersonId)
    {
        var model = new StoreSalesPersonAssignmentCreateModel { SalesPersonId = salesPersonId };

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(x => x.SalesPersonId);
    }
}
