using AdventureWorks.Models.Features.Production;
using FluentValidation;

namespace AdventureWorks.Application.Features.Production.Validators;

public sealed class UpdateProductValidator : ProductBaseModelValidator<ProductUpdateModel>
{
    public UpdateProductValidator()
    {
        RuleFor(a => a.Id)
            .GreaterThan(0)
            .WithErrorCode("Rule-00").WithMessage(MessageProductIdInvalid);
    }

    public static string MessageProductIdInvalid => "Product id must be greater than zero";
}
