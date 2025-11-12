using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for UpdateEmployeeCommand.
/// Updates both EmployeeEntity and PersonEntity tables.
/// </summary>
public sealed class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IValidator<EmployeeUpdateModel> validator,
    IPublisher publisher)
        : IRequestHandler<UpdateEmployeeCommand, Unit>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeeUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task<Unit> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var employeeEntity = await _employeeRepository.GetEmployeeByIdAsync(request.Model.Id, cancellationToken);

        if (employeeEntity == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.Model.Id} not found.");
        }

        var personEntity = employeeEntity.PersonBusinessEntity;
        personEntity.FirstName = request.Model.FirstName;
        personEntity.LastName = request.Model.LastName;
        personEntity.MiddleName = request.Model.MiddleName;
        personEntity.Title = request.Model.Title;
        personEntity.Suffix = request.Model.Suffix;
        personEntity.ModifiedDate = request.ModifiedDate;
        employeeEntity.MaritalStatus = request.Model.MaritalStatus;
        employeeEntity.Gender = request.Model.Gender;
        employeeEntity.ModifiedDate = request.ModifiedDate;

        await _employeeRepository.UpdateAsync(employeeEntity, cancellationToken);

        await _publisher.Publish(new EntityChangedNotification
        {
            EntityType = "Employee",
            EntityId = request.Model.Id,
            Action = "Updated",
            UserName = request.UserName,
            Timestamp = request.ModifiedDate
        }, cancellationToken);

        return Unit.Value;
    }
}
