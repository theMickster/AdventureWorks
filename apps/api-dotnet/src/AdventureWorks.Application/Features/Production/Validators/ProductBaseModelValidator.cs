using AdventureWorks.Models.Features.Production;
using FluentValidation;

namespace AdventureWorks.Application.Features.Production.Validators;

public class ProductBaseModelValidator<T> : AbstractValidator<T> where T : ProductBaseModel
{
    public ProductBaseModelValidator()
    {
        RuleFor(a => a.Name)
            .NotEmpty()
            .WithErrorCode("Rule-01").WithMessage(MessageProductNameEmpty)
            .MaximumLength(50)
            .WithErrorCode("Rule-02").WithMessage(MessageProductNameLength);

        RuleFor(a => a.ProductNumber)
            .NotEmpty()
            .WithErrorCode("Rule-03").WithMessage(MessageProductNumberEmpty)
            .MaximumLength(25)
            .WithErrorCode("Rule-04").WithMessage(MessageProductNumberLength);

        RuleFor(a => a.SafetyStockLevel)
            .GreaterThan((short)0)
            .WithErrorCode("Rule-05").WithMessage(MessageSafetyStockLevelInvalid);

        RuleFor(a => a.ReorderPoint)
            .GreaterThan((short)0)
            .WithErrorCode("Rule-06").WithMessage(MessageReorderPointInvalid);

        RuleFor(a => a.StandardCost)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("Rule-07").WithMessage(MessageStandardCostInvalid);

        RuleFor(a => a.ListPrice)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("Rule-08").WithMessage(MessageListPriceInvalid);

        RuleFor(a => a.DaysToManufacture)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("Rule-09").WithMessage(MessageDaysToManufactureInvalid);

        RuleFor(a => a.Weight)
            .GreaterThan(0)
            .When(a => a.Weight.HasValue)
            .WithErrorCode("Rule-10").WithMessage(MessageWeightInvalid);

        RuleFor(a => a.ProductLine)
            .Must(x => new[] { "R", "M", "T", "S", null }.Contains(x))
            .WithErrorCode("Rule-11").WithMessage(MessageProductLineInvalid);

        RuleFor(a => a.Class)
            .Must(x => new[] { "H", "M", "L", null }.Contains(x))
            .WithErrorCode("Rule-12").WithMessage(MessageClassInvalid);

        RuleFor(a => a.Style)
            .Must(x => new[] { "U", "M", "W", null }.Contains(x))
            .WithErrorCode("Rule-13").WithMessage(MessageStyleInvalid);

        RuleFor(a => a.SellEndDate)
            .GreaterThanOrEqualTo(a => a.SellStartDate)
            .When(a => a.SellEndDate.HasValue)
            .WithErrorCode("Rule-14").WithMessage(MessageSellEndDateInvalid);
    }

    public static string MessageProductNameEmpty => "Product name cannot be null, empty, or whitespace";

    public static string MessageProductNameLength => "Product name cannot be greater than 50 characters";

    public static string MessageProductNumberEmpty => "Product number cannot be null, empty, or whitespace";

    public static string MessageProductNumberLength => "Product number cannot be greater than 25 characters";

    public static string MessageSafetyStockLevelInvalid => "Safety stock level must be greater than zero";

    public static string MessageReorderPointInvalid => "Reorder point must be greater than zero";

    public static string MessageStandardCostInvalid => "Standard cost must be greater than or equal to zero";

    public static string MessageListPriceInvalid => "List price must be greater than or equal to zero";

    public static string MessageDaysToManufactureInvalid => "Days to manufacture must be greater than or equal to zero";

    public static string MessageWeightInvalid => "Weight must be greater than zero when specified";

    public static string MessageProductLineInvalid => "Product line must be one of: R, M, T, S, or null";

    public static string MessageClassInvalid => "Class must be one of: H, M, L, or null";

    public static string MessageStyleInvalid => "Style must be one of: U, M, W, or null";

    public static string MessageSellEndDateInvalid => "Sell end date must be greater than or equal to sell start date";
}
