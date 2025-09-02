using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

public sealed class ReassignStoreSalesPersonValidator : AbstractValidator<StoreSalesPersonAssignmentCreateModel>
{
    public ReassignStoreSalesPersonValidator()
    {
        RuleFor(x => x.SalesPersonId)
            .GreaterThan(0)
            .WithErrorCode("Rule-01")
            .WithMessage(MessageSalesPersonIdInvalid);
    }

    public static string MessageSalesPersonIdInvalid => "A valid sales person id must be specified.";
}
