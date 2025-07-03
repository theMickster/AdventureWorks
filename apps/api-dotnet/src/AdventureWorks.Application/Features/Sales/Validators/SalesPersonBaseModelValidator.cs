using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

public class SalesPersonBaseModelValidator<T> : AbstractValidator<T> where T : SalesPersonBaseModel
{
    public SalesPersonBaseModelValidator()
    {
        RuleFor(x => x.CommissionPct)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("Rule-01").WithMessage(MessageCommissionPctNonNegative)
            .LessThanOrEqualTo(1)
            .WithErrorCode("Rule-02").WithMessage(MessageCommissionPctMaxValue);

        RuleFor(x => x.Bonus)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("Rule-03").WithMessage(MessageBonusNonNegative);

        RuleFor(x => x.SalesQuota)
            .GreaterThan(0)
            .When(x => x.SalesQuota.HasValue)
            .WithErrorCode("Rule-04").WithMessage(MessageSalesQuotaPositive);

        RuleFor(x => x.TerritoryId)
            .GreaterThan(0)
            .When(x => x.TerritoryId.HasValue)
            .WithErrorCode("Rule-05").WithMessage(MessageTerritoryIdPositive);
    }

    public static string MessageCommissionPctNonNegative => "Commission percentage must be greater than or equal to 0";

    public static string MessageCommissionPctMaxValue => "Commission percentage cannot exceed 100% (1.0)";

    public static string MessageBonusNonNegative => "Bonus must be greater than or equal to 0";

    public static string MessageSalesQuotaPositive => "Sales quota must be greater than 0 when specified";

    public static string MessageTerritoryIdPositive => "Territory ID must be greater than 0 when specified";
}
