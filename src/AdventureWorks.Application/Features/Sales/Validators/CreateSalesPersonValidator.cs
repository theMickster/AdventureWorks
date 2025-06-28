using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

public sealed class CreateSalesPersonValidator : SalesPersonBaseModelValidator<SalesPersonCreateModel>
{
    public CreateSalesPersonValidator()
    {
        RuleFor(x => x.BusinessEntityId)
            .GreaterThan(0)
            .WithErrorCode("Rule-10").WithMessage(MessageBusinessEntityIdRequired);
    }

    public static string MessageBusinessEntityIdRequired => "Business Entity ID must be greater than 0";
}
