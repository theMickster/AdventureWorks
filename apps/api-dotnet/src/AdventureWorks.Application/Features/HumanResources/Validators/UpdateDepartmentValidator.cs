using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

public sealed class UpdateDepartmentValidator : DepartmentBaseModelValidator<DepartmentUpdateModel>
{
    public UpdateDepartmentValidator(IDepartmentRepository departmentRepository)
    {
        ArgumentNullException.ThrowIfNull(departmentRepository);

        RuleFor(x => x.Id)
            .GreaterThan((short)0)
            .WithErrorCode("Rule-05").WithMessage(MessageIdInvalid);

        RuleFor(x => x)
            .MustAsync(async (model, ct) =>
                !await departmentRepository.ExistsByNameExcludingIdAsync(model.Name.Trim(), model.Id, ct))
            .WithErrorCode("Rule-06").WithMessage(MessageNameAlreadyInUse);
    }

    public static string MessageIdInvalid => "A valid department ID must be provided.";
    public static string MessageNameAlreadyInUse => "A department with this name already exists.";
}
