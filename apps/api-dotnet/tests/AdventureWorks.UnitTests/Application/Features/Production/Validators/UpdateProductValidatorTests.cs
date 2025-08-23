using AdventureWorks.Application.Features.Production.Validators;
using AdventureWorks.Models.Features.Production;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Production.Validators;

[ExcludeFromCodeCoverage]
public sealed class UpdateProductValidatorTests : UnitTestBase
{
    private readonly UpdateProductValidator _sut = new();

    private static ProductUpdateModel ValidModel => new()
    {
        Id = 1,
        Name = "Test Product",
        ProductNumber = "TP-0001",
        SafetyStockLevel = 100,
        ReorderPoint = 75,
        StandardCost = 10.00m,
        ListPrice = 25.00m,
        DaysToManufacture = 1,
        SellStartDate = new DateTime(2024, 1, 1)
    };

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rule00_id_invalid(int id)
    {
        var model = ValidModel;
        model.Id = id;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Rule00_id_valid()
    {
        var model = ValidModel;
        model.Id = 1;
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validator_succeeds_when_all_data_is_valid()
    {
        var result = _sut.TestValidate(ValidModel);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_error_message_is_correct()
    {
        UpdateProductValidator.MessageProductIdInvalid.Should().Be("Product id must be greater than zero");
    }
}
