using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

public sealed class UpdateSalesPersonValidator : SalesPersonBaseModelValidator<SalesPersonUpdateModel>
{
    public UpdateSalesPersonValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode("Rule-11").WithMessage(MessageIdRequired);
    }

    public static string MessageIdRequired => "Sales Person ID must be greater than 0";
}
