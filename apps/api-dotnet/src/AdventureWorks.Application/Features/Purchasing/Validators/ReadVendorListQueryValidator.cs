using AdventureWorks.Application.Features.Purchasing.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Purchasing.Validators;

/// <summary>
/// Validator for <see cref="ReadVendorListQuery"/>.
/// </summary>
public sealed class ReadVendorListQueryValidator : AbstractValidator<ReadVendorListQuery>
{
    public ReadVendorListQueryValidator()
    {
        RuleFor(x => x.Parameters)
            .NotNull().WithErrorCode("Rule-01").WithMessage("Parameters cannot be null");

        // Note: PageNumber and PageSize are clamped in the init property, so no need to validate them

        When(x => x.Parameters != null, () =>
        {
            RuleFor(x => x.Parameters.CreditRating)
                .InclusiveBetween((byte)1, (byte)5)
                .When(x => x.Parameters.CreditRating.HasValue)
                .WithErrorCode("Rule-02")
                .WithMessage("CreditRating must be between 1 and 5");
        });
    }
}
