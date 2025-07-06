using AdventureWorks.Common.Constants;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>
/// Validator for EmployeeHireModel with business rules for hire workflow.
/// </summary>
public sealed class HireEmployeeValidator : AbstractValidator<EmployeeHireModel>
{
    public HireEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("EmployeeId must be greater than 0.");

        RuleFor(x => x.HireDate)
            .NotEmpty()
            .WithMessage("HireDate is required.")
            .Must(date => date <= DateTime.UtcNow.AddDays(HumanResourcesConstants.MaximumFutureHireDays))
            .WithMessage($"HireDate cannot be more than {HumanResourcesConstants.MaximumFutureHireDays} days in the future.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan((short)0)
            .WithMessage("DepartmentId must be greater than 0.");

        RuleFor(x => x.ShiftId)
            .InclusiveBetween(HumanResourcesConstants.MinimumShiftId, HumanResourcesConstants.MaximumShiftId)
            .WithMessage($"ShiftId must be between {HumanResourcesConstants.MinimumShiftId} and {HumanResourcesConstants.MaximumShiftId} (1=Day, 2=Evening, 3=Night).");

        RuleFor(x => x.InitialPayRate)
            .GreaterThan(0)
            .WithMessage("InitialPayRate must be greater than 0.")
            .LessThanOrEqualTo(HumanResourcesConstants.MaximumPayRate)
            .WithMessage($"InitialPayRate cannot exceed ${HumanResourcesConstants.MaximumPayRate:F2}.");

        RuleFor(x => x.PayFrequency)
            .InclusiveBetween(HumanResourcesConstants.PayFrequencyMonthly, HumanResourcesConstants.PayFrequencyBiWeekly)
            .WithMessage($"PayFrequency must be {HumanResourcesConstants.PayFrequencyMonthly} (Monthly) or {HumanResourcesConstants.PayFrequencyBiWeekly} (Bi-weekly).");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0)
            .When(x => x.ManagerId.HasValue)
            .WithMessage("ManagerId must be greater than 0 when provided.");

        RuleFor(x => x.InitialVacationHours)
            .GreaterThanOrEqualTo((short)0)
            .WithMessage("InitialVacationHours must be greater than or equal to 0.")
            .LessThanOrEqualTo(HumanResourcesConstants.MaximumVacationHours)
            .WithMessage($"InitialVacationHours cannot exceed {HumanResourcesConstants.MaximumVacationHours} hours.");

        RuleFor(x => x.InitialSickLeaveHours)
            .GreaterThanOrEqualTo((short)0)
            .WithMessage("InitialSickLeaveHours must be greater than or equal to 0.")
            .LessThanOrEqualTo(HumanResourcesConstants.MaximumSickLeaveHours)
            .WithMessage($"InitialSickLeaveHours cannot exceed {HumanResourcesConstants.MaximumSickLeaveHours} hours.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}
