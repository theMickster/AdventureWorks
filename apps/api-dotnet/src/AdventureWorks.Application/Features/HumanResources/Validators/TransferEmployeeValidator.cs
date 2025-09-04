using AdventureWorks.Common.Constants;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

/// <summary>Validates <see cref="EmployeeTransferModel"/> for department transfer requests.</summary>
public sealed class TransferEmployeeValidator : AbstractValidator<EmployeeTransferModel>
{
    public static readonly string DepartmentIdInvalidMessage = "DepartmentId must be greater than 0.";
    public static readonly string ShiftIdInvalidMessage =
        $"ShiftId must be between {HumanResourcesConstants.MinimumShiftId} and {HumanResourcesConstants.MaximumShiftId}.";

    public TransferEmployeeValidator()
    {
        RuleFor(x => x.DepartmentId)
            .GreaterThan((short)0)
            .WithMessage(DepartmentIdInvalidMessage);

        RuleFor(x => x.ShiftId)
            .InclusiveBetween(HumanResourcesConstants.MinimumShiftId, HumanResourcesConstants.MaximumShiftId)
            .WithErrorCode("Rule-02")
            .WithMessage(ShiftIdInvalidMessage);
    }
}
