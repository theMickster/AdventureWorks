using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;

namespace AdventureWorks.Application.Features.HumanResources.Validators;

public sealed class CreateDepartmentValidator : DepartmentBaseModelValidator<DepartmentCreateModel>
{
    public CreateDepartmentValidator(IDepartmentRepository departmentRepository)
    {
        ArgumentNullException.ThrowIfNull(departmentRepository);

        RuleFor(x => x.Name)
            .MustAsync(async (name, ct) => !await departmentRepository.ExistsByNameAsync(name.Trim(), ct))
            .WithErrorCode("Rule-05").WithMessage(MessageNameAlreadyInUse);
    }

    public static string MessageNameAlreadyInUse => "A department with this name already exists.";
}
