using AdventureWorks.Models.Features.Production;
using FluentValidation;

namespace AdventureWorks.Application.Features.Production.Validators;

public sealed class CreateProductValidator : ProductBaseModelValidator<ProductCreateModel>
{
    public CreateProductValidator()
    {
        RuleFor(a => a.SellStartDate)
            .NotEmpty()
            .WithErrorCode("Rule-15").WithMessage(MessageSellStartDateEmpty);
    }

    public static string MessageSellStartDateEmpty => "Sell start date is required";
}
