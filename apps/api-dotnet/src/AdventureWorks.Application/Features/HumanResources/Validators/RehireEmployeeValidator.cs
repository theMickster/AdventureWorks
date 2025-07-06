using AdventureWorks.Common.Constants;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Validator for EmployeeRehireModel with business rules for rehire workflow.
/// </summary>
public sealed class RehireEmployeeValidator : AbstractValidator<EmployeeRehireModel>
{
    public RehireEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("EmployeeId must be greater than 0.");

        RuleFor(x => x.RehireDate)
            .NotEmpty()
            .WithMessage("RehireDate is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("RehireDate cannot be in the past.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan((short)0)
            .WithMessage("DepartmentId must be greater than 0.");

        RuleFor(x => x.ShiftId)
            .InclusiveBetween(HumanResourcesConstants.MinimumShiftId, HumanResourcesConstants.MaximumShiftId)
            .WithMessage($"ShiftId must be between {HumanResourcesConstants.MinimumShiftId} and {HumanResourcesConstants.MaximumShiftId} (1=Day, 2=Evening, 3=Night).");

        RuleFor(x => x.PayRate)
            .GreaterThan(0)
            .WithMessage("PayRate must be greater than 0.")
            .LessThanOrEqualTo(HumanResourcesConstants.MaximumPayRate)
            .WithMessage($"PayRate cannot exceed ${HumanResourcesConstants.MaximumPayRate:F2}.");

        RuleFor(x => x.PayFrequency)
            .InclusiveBetween(HumanResourcesConstants.PayFrequencyMonthly, HumanResourcesConstants.PayFrequencyBiWeekly)
            .WithMessage($"PayFrequency must be {HumanResourcesConstants.PayFrequencyMonthly} (Monthly) or {HumanResourcesConstants.PayFrequencyBiWeekly} (Bi-weekly).");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0)
            .When(x => x.ManagerId.HasValue)
            .WithMessage("ManagerId must be greater than 0 when provided.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}
