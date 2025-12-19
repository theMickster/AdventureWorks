using AdventureWorks.Application.Features.Production.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Production.Validators;

/// <summary>
/// Validator for ReadWorkOrderListQuery.
/// </summary>
public sealed class ReadWorkOrderListQueryValidator : AbstractValidator<ReadWorkOrderListQuery>
{
    public ReadWorkOrderListQueryValidator()
    {
        RuleFor(x => x.Parameters)
            .NotNull().WithErrorCode("Rule-01").WithMessage("Parameters cannot be null");

        // Note: PageNumber and PageSize are clamped in the init property, so no need to validate them

        When(x => x.SearchModel != null, () =>
        {
            RuleFor(x => x.SearchModel!.StartDate)
                .LessThanOrEqualTo(x => x.SearchModel!.EndDate)
                .When(x => x.SearchModel!.StartDate.HasValue && x.SearchModel!.EndDate.HasValue)
                .WithErrorCode("Rule-02")
                .WithMessage("StartDate must be less than or equal to EndDate");

            RuleFor(x => x.SearchModel!.ProductId)
                .GreaterThan(0)
                .When(x => x.SearchModel!.ProductId.HasValue)
                .WithErrorCode("Rule-03")
                .WithMessage("ProductId must be greater than 0");

            RuleFor(x => x.SearchModel!.ScrapReasonId)
                .GreaterThan((short)0)
                .When(x => x.SearchModel!.ScrapReasonId.HasValue)
                .WithErrorCode("Rule-04")
                .WithMessage("ScrapReasonId must be greater than 0");
        });
    }
}
