using AdventureWorks.Application.Features.Sales.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validates <see cref="GetSalesOrderAnalyticsQuery"/>. All rules are conditional on <c>Filter != null</c>:
/// Rule-04 date ordering, Rule-05/06 positive IDs, Rule-07 status 1–6, Rule-08 AccountNumber max 15 chars.
/// </summary>
public sealed class GetSalesOrderAnalyticsQueryValidator : AbstractValidator<GetSalesOrderAnalyticsQuery>
{
    public GetSalesOrderAnalyticsQueryValidator()
    {
        When(x => x.Filter != null, () =>
        {
            RuleFor(x => x.Filter!.OrderDateFrom)
                .LessThanOrEqualTo(x => x.Filter!.OrderDateTo)
                .When(x => x.Filter!.OrderDateFrom.HasValue && x.Filter!.OrderDateTo.HasValue)
                .WithErrorCode("Rule-04")
                .WithMessage("OrderDateFrom must be less than or equal to OrderDateTo");

            RuleFor(x => x.Filter!.SalesPersonId)
                .GreaterThan(0)
                .When(x => x.Filter!.SalesPersonId.HasValue)
                .WithErrorCode("Rule-05")
                .WithMessage("SalesPersonId must be greater than 0");

            RuleFor(x => x.Filter!.TerritoryId)
                .GreaterThan(0)
                .When(x => x.Filter!.TerritoryId.HasValue)
                .WithErrorCode("Rule-06")
                .WithMessage("TerritoryId must be greater than 0");

            RuleFor(x => x.Filter!.Status)
                .InclusiveBetween((byte)1, (byte)6)
                .When(x => x.Filter!.Status.HasValue)
                .WithErrorCode("Rule-07")
                .WithMessage("Status must be between 1 and 6");

            RuleFor(x => x.Filter!.AccountNumber)
                .MaximumLength(15)
                .WithErrorCode("Rule-08")
                .WithMessage("AccountNumber must not exceed 15 characters.")
                .When(x => x.Filter?.AccountNumber != null);
        });
    }
}
