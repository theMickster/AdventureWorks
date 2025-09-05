using AdventureWorks.Common.Constants;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>Validates <see cref="EmployeePayChangeCreateModel"/> for pay change requests.</summary>
public sealed class RecordPayChangeValidator : AbstractValidator<EmployeePayChangeCreateModel>
{
    private static readonly HashSet<byte> ValidPayFrequencies = new()
    {
        HumanResourcesConstants.PayFrequencyMonthly,
        HumanResourcesConstants.PayFrequencyBiWeekly
    };

    public static readonly string MessageRateBelowMinimum =
        $"Rate must be at least {HumanResourcesConstants.PayHistoryMinimumRate:C}.";
    public static readonly string MessageRateExceedsMaximum =
        $"Rate cannot exceed {HumanResourcesConstants.PayHistoryMaximumRate:C}.";
    public static readonly string MessagePayFrequencyInvalid =
        $"PayFrequency must be {HumanResourcesConstants.PayFrequencyMonthly} (Monthly) or {HumanResourcesConstants.PayFrequencyBiWeekly} (Bi-Weekly).";
    public static readonly string MessageInactiveEmployee =
        "Pay changes can only be recorded for active employees.";

    public RecordPayChangeValidator()
    {
        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(HumanResourcesConstants.PayHistoryMinimumRate)
            .WithErrorCode("Rule-01")
            .WithMessage(MessageRateBelowMinimum);

        RuleFor(x => x.Rate)
            .LessThanOrEqualTo(HumanResourcesConstants.PayHistoryMaximumRate)
            .WithErrorCode("Rule-02")
            .WithMessage(MessageRateExceedsMaximum);

        RuleFor(x => x.PayFrequency)
            .Must(x => ValidPayFrequencies.Contains(x))
            .WithErrorCode("Rule-03")
            .WithMessage(MessagePayFrequencyInvalid);
    }
}
