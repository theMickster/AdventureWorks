using AdventureWorks.Application.Features.Sales.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validator for ReadSalesOrderListQuery.
/// </summary>
public sealed class ReadSalesOrderListQueryValidator : AbstractValidator<ReadSalesOrderListQuery>
{
    public ReadSalesOrderListQueryValidator()
    {
        RuleFor(x => x.Parameters)
            .NotNull().WithErrorCode("Rule-01").WithMessage("Parameters cannot be null");

        // Note: PageNumber and PageSize are clamped in the init property, so no need to validate them

        When(x => x.SearchModel != null, () =>
        {
            RuleFor(x => x.SearchModel!.OrderDateFrom)
                .LessThanOrEqualTo(x => x.SearchModel!.OrderDateTo)
                .When(x => x.SearchModel!.OrderDateFrom.HasValue && x.SearchModel!.OrderDateTo.HasValue)
                .WithErrorCode("Rule-04")
                .WithMessage("OrderDateFrom must be less than or equal to OrderDateTo");

            RuleFor(x => x.SearchModel!.SalesPersonId)
                .GreaterThan(0)
                .When(x => x.SearchModel!.SalesPersonId.HasValue)
                .WithErrorCode("Rule-05")
                .WithMessage("SalesPersonId must be greater than 0");

            RuleFor(x => x.SearchModel!.TerritoryId)
                .GreaterThan(0)
                .When(x => x.SearchModel!.TerritoryId.HasValue)
                .WithErrorCode("Rule-06")
                .WithMessage("TerritoryId must be greater than 0");

            RuleFor(x => x.SearchModel!.Status)
                .InclusiveBetween((byte)1, (byte)6)
                .When(x => x.SearchModel!.Status.HasValue)
                .WithErrorCode("Rule-07")
                .WithMessage("Status must be between 1 and 6");

            RuleFor(x => x.SearchModel!.AccountNumber)
                .MaximumLength(15)
                .WithErrorCode("Rule-08")
                .WithMessage("AccountNumber must not exceed 15 characters.")
                .When(x => x.SearchModel?.AccountNumber != null);
        });
    }
}
