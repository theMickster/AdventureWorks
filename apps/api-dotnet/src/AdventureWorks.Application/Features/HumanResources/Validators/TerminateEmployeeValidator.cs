using AdventureWorks.Common.Constants;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Validator for EmployeeTerminateModel with business rules for termination workflow.
/// </summary>
public sealed class TerminateEmployeeValidator : AbstractValidator<EmployeeTerminateModel>
{
    private static readonly string[] ValidTerminationTypes =
        { "Voluntary", "Involuntary", "Retirement", "Layoff" };

    public TerminateEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("EmployeeId must be greater than 0.");

        RuleFor(x => x.TerminationDate)
            .NotEmpty()
            .WithMessage("TerminationDate is required.")
            .Must(date => date <= DateTime.UtcNow.AddDays(HumanResourcesConstants.MaximumFutureTerminationDays))
            .WithMessage($"TerminationDate cannot be more than {HumanResourcesConstants.MaximumFutureTerminationDays} days in the future.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Termination reason is required.")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters.");

        RuleFor(x => x.TerminationType)
            .NotEmpty()
            .WithMessage("TerminationType is required.")
            .Must(type => ValidTerminationTypes.Contains(type))
            .WithMessage("TerminationType must be one of: Voluntary, Involuntary, Retirement, Layoff.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters.");
    }
}
