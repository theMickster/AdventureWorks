using AdventureWorks.Application.Features.Purchasing.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Purchasing.Validators;

/// <summary>
/// Validator for <see cref="ReadVendorPurchaseOrdersQuery"/>.
/// </summary>
public sealed class ReadVendorPurchaseOrdersQueryValidator : AbstractValidator<ReadVendorPurchaseOrdersQuery>
{
    public ReadVendorPurchaseOrdersQueryValidator()
    {
        RuleFor(x => x.Parameters)
            .NotNull().WithErrorCode("Rule-01").WithMessage("Parameters cannot be null");

        // Note: PageNumber and PageSize are clamped in the init property, so no need to validate them

        When(x => x.Parameters != null, () =>
        {
            RuleFor(x => x.Parameters.Status)
                .InclusiveBetween((byte)1, (byte)4)
                .When(x => x.Parameters.Status.HasValue)
                .WithErrorCode("Rule-02")
                .WithMessage("Status must be between 1 and 4");

            RuleFor(x => x.Parameters)
                .Must(p => !p.StartDate.HasValue || !p.EndDate.HasValue || p.StartDate.Value <= p.EndDate.Value)
                .WithErrorCode("Rule-03")
                .WithMessage("StartDate must be on or before EndDate");
        });
    }
}
