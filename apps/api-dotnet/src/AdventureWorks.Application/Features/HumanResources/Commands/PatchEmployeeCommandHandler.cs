using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for PatchEmployeeCommand.
/// Applies JSON Patch operations to an employee's personal information.
/// Updates both PersonEntity and EmployeeEntity tables.
/// </summary>
public sealed class PatchEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IValidator<EmployeeUpdateModel> validator)
        : IRequestHandler<PatchEmployeeCommand, Unit>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeeUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Unit> Handle(PatchEmployeeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.PatchDocument);

        // Get the employee entity for update operations
        var employeeEntity = await _employeeRepository.GetEmployeeByIdAsync(request.EmployeeId, cancellationToken);

        if (employeeEntity == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found.");
        }

        var personEntity = employeeEntity.PersonBusinessEntity;

        // Create a model from the current employee and person entities
        var employeeUpdateModel = new EmployeeUpdateModel
        {
            Id = employeeEntity.BusinessEntityId,
            FirstName = personEntity.FirstName,
            LastName = personEntity.LastName,
            MiddleName = personEntity.MiddleName,
            Title = personEntity.Title,
            Suffix = personEntity.Suffix,
            MaritalStatus = employeeEntity.MaritalStatus,
            Gender = employeeEntity.Gender
        };

        // Apply the patch operations
        request.PatchDocument.ApplyTo(employeeUpdateModel);

        // Ensure Id field remains immutable (restore original if patched)
        if (employeeUpdateModel.Id != request.EmployeeId)
        {
            employeeUpdateModel.Id = request.EmployeeId;
        }

        // Validate the patched model
        await _validator.ValidateAndThrowAsync(employeeUpdateModel, cancellationToken);

        // Apply changes to PersonEntity
        personEntity.FirstName = employeeUpdateModel.FirstName;
        personEntity.LastName = employeeUpdateModel.LastName;
        personEntity.MiddleName = employeeUpdateModel.MiddleName;
        personEntity.Title = employeeUpdateModel.Title;
        personEntity.Suffix = employeeUpdateModel.Suffix;
        personEntity.ModifiedDate = request.ModifiedDate;

        // Apply changes to EmployeeEntity
        employeeEntity.MaritalStatus = employeeUpdateModel.MaritalStatus;
        employeeEntity.Gender = employeeUpdateModel.Gender;
        employeeEntity.ModifiedDate = request.ModifiedDate;

        await _employeeRepository.UpdateAsync(employeeEntity);

        return Unit.Value;
    }
}
