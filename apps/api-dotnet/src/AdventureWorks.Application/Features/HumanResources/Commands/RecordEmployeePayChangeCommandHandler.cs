using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>Handles <see cref="RecordEmployeePayChangeCommand"/> by inserting a new pay history record.</summary>
public sealed class RecordEmployeePayChangeCommandHandler(
    IEmployeeRepository employeeRepository,
    IValidator<EmployeePayChangeCreateModel> validator,
    ILogger<RecordEmployeePayChangeCommandHandler> logger)
    : IRequestHandler<RecordEmployeePayChangeCommand, Unit>
{
    private readonly IEmployeeRepository _employeeRepository =
        employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeePayChangeCreateModel> _validator =
        validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<RecordEmployeePayChangeCommandHandler> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(RecordEmployeePayChangeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var employee = await _employeeRepository.GetEmployeeByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            _logger.LogWarning("Employee {EmployeeId} not found for pay change.", request.EmployeeId);
            throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found.");
        }

        if (!employee.CurrentFlag)
        {
            _logger.LogWarning(
                "Pay change rejected for employee {EmployeeId}. Rule={RuleCode}: Employee is not active.",
                request.EmployeeId, "Rule-04");
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(EmployeePayChangeCreateModel), RecordPayChangeValidator.MessageInactiveEmployee)
                {
                    ErrorCode = "Rule-04"
                }
            });
        }

        var record = new EmployeePayHistory
        {
            BusinessEntityId = request.EmployeeId,
            RateChangeDate = request.RateChangeDate,
            Rate = request.Model.Rate,
            PayFrequency = request.Model.PayFrequency,
            ModifiedDate = request.ModifiedDate
        };

        await _employeeRepository.RecordPayChangeAsync(record, cancellationToken);

        _logger.LogInformation("Pay change recorded for employee {EmployeeId}: Rate={Rate}, Frequency={Frequency}.",
            request.EmployeeId, request.Model.Rate, request.Model.PayFrequency);

        return Unit.Value;
    }
}
