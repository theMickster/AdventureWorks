using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

public abstract class DepartmentBaseModelValidator<T> : AbstractValidator<T>
    where T : DepartmentBaseModel
{
    protected DepartmentBaseModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("Rule-01").WithMessage(MessageNameEmpty)
            .MaximumLength(50).WithErrorCode("Rule-02").WithMessage(MessageNameLength);

        RuleFor(x => x.GroupName)
            .NotEmpty().WithErrorCode("Rule-03").WithMessage(MessageGroupNameEmpty)
            .MaximumLength(50).WithErrorCode("Rule-04").WithMessage(MessageGroupNameLength);
    }

    public static string MessageNameEmpty => "Department name cannot be null, empty, or whitespace.";
    public static string MessageNameLength => "Department name cannot exceed 50 characters.";
    public static string MessageGroupNameEmpty => "Department group name cannot be null, empty, or whitespace.";
    public static string MessageGroupNameLength => "Department group name cannot exceed 50 characters.";
}
