using AdventureWorks.Application.Features.Production.Validators;
using AdventureWorks.Models.Features.Production;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.Production.Validators;

[ExcludeFromCodeCoverage]
public sealed class CreateProductValidatorTests : UnitTestBase
{
    private readonly CreateProductValidator _sut = new();

    private static ProductCreateModel ValidModel => new()
    {
        Name = "Test Product",
        ProductNumber = "TP-0001",
        SafetyStockLevel = 100,
        ReorderPoint = 75,
        StandardCost = 10.00m,
        ListPrice = 25.00m,
        DaysToManufacture = 1,
        SellStartDate = new DateTime(2024, 1, 1)
    };

    [Fact]
    public void Validator_error_messages_are_correct()
    {
        using (new AssertionScope())
        {
            CreateProductValidator.MessageProductNameEmpty.Should().Be("Product name cannot be null, empty, or whitespace");
            CreateProductValidator.MessageProductNameLength.Should().Be("Product name cannot be greater than 50 characters");
            CreateProductValidator.MessageProductNumberEmpty.Should().Be("Product number cannot be null, empty, or whitespace");
            CreateProductValidator.MessageProductNumberLength.Should().Be("Product number cannot be greater than 25 characters");
            CreateProductValidator.MessageSafetyStockLevelInvalid.Should().Be("Safety stock level must be greater than zero");
            CreateProductValidator.MessageReorderPointInvalid.Should().Be("Reorder point must be greater than zero");
            CreateProductValidator.MessageStandardCostInvalid.Should().Be("Standard cost must be greater than or equal to zero");
            CreateProductValidator.MessageListPriceInvalid.Should().Be("List price must be greater than or equal to zero");
            CreateProductValidator.MessageDaysToManufactureInvalid.Should().Be("Days to manufacture must be greater than or equal to zero");
            CreateProductValidator.MessageWeightInvalid.Should().Be("Weight must be greater than zero when specified");
            CreateProductValidator.MessageProductLineInvalid.Should().Be("Product line must be one of: R, M, T, S, or null");
            CreateProductValidator.MessageClassInvalid.Should().Be("Class must be one of: H, M, L, or null");
            CreateProductValidator.MessageStyleInvalid.Should().Be("Style must be one of: U, M, W, or null");
            CreateProductValidator.MessageSellEndDateInvalid.Should().Be("Sell end date must be greater than or equal to sell start date");
            CreateProductValidator.MessageSellStartDateEmpty.Should().Be("Sell start date is required");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rule01_name_empty(string name)
    {
        var model = ValidModel;
        model.Name = name;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Rule02_name_too_long()
    {
        var model = ValidModel;
        model.Name = new string('a', 51);
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rule03_product_number_empty(string number)
    {
        var model = ValidModel;
        model.ProductNumber = number;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ProductNumber);
    }

    [Fact]
    public void Rule04_product_number_too_long()
    {
        var model = ValidModel;
        model.ProductNumber = new string('X', 26);
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ProductNumber);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    public void Rule05_safety_stock_level_invalid(short value)
    {
        var model = ValidModel;
        model.SafetyStockLevel = value;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.SafetyStockLevel);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    public void Rule06_reorder_point_invalid(short value)
    {
        var model = ValidModel;
        model.ReorderPoint = value;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ReorderPoint);
    }

    [Fact]
    public void Rule07_standard_cost_negative()
    {
        var model = ValidModel;
        model.StandardCost = -1;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.StandardCost);
    }

    [Fact]
    public void Rule08_list_price_negative()
    {
        var model = ValidModel;
        model.ListPrice = -1;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ListPrice);
    }

    [Fact]
    public void Rule09_days_to_manufacture_negative()
    {
        var model = ValidModel;
        model.DaysToManufacture = -1;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.DaysToManufacture);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rule10_weight_invalid_when_specified(decimal weight)
    {
        var model = ValidModel;
        model.Weight = weight;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Weight);
    }

    [Fact]
    public void Rule10_weight_null_is_valid()
    {
        var model = ValidModel;
        model.Weight = null;
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Weight);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Z")]
    public void Rule11_product_line_invalid(string productLine)
    {
        var model = ValidModel;
        model.ProductLine = productLine;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ProductLine);
    }

    [Theory]
    [InlineData("R")]
    [InlineData("M")]
    [InlineData("T")]
    [InlineData("S")]
    [InlineData(null)]
    public void Rule11_product_line_valid(string? productLine)
    {
        var model = ValidModel;
        model.ProductLine = productLine;
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.ProductLine);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Z")]
    public void Rule12_class_invalid(string cls)
    {
        var model = ValidModel;
        model.Class = cls;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Class);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Z")]
    public void Rule13_style_invalid(string style)
    {
        var model = ValidModel;
        model.Style = style;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Style);
    }

    [Fact]
    public void Rule14_sell_end_date_before_start()
    {
        var model = ValidModel;
        model.SellStartDate = new DateTime(2024, 6, 1);
        model.SellEndDate = new DateTime(2024, 1, 1);
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.SellEndDate);
    }

    [Fact]
    public void Rule14_sell_end_date_null_is_valid()
    {
        var model = ValidModel;
        model.SellEndDate = null;
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.SellEndDate);
    }

    [Fact]
    public void Rule15_sell_start_date_empty()
    {
        var model = ValidModel;
        model.SellStartDate = default;
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.SellStartDate);
    }

    [Fact]
    public void Validator_succeeds_when_all_data_is_valid()
    {
        var result = _sut.TestValidate(ValidModel);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
