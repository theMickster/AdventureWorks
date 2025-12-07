using AdventureWorks.Application.Features.Sales.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validator for ReadCustomerListQuery.
/// </summary>
public sealed class ReadCustomerListQueryValidator : AbstractValidator<ReadCustomerListQuery>
{
    public ReadCustomerListQueryValidator()
    {
        RuleFor(x => x.Parameters)
            .NotNull().WithErrorCode("Rule-01").WithMessage("Parameters cannot be null");

        // Note: PageNumber and PageSize are clamped in the init property, so no need to validate them
    }
}
