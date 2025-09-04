using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Handles <see cref="ReadEmployeeDepartmentHistoryQuery"/> by returning an employee's complete department assignment history.</summary>
public sealed class ReadEmployeeDepartmentHistoryQueryHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository)
    : IRequestHandler<ReadEmployeeDepartmentHistoryQuery, IReadOnlyList<EmployeeDepartmentHistoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _employeeRepository =
        employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public async Task<IReadOnlyList<EmployeeDepartmentHistoryModel>> Handle(
        ReadEmployeeDepartmentHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Two-call pattern is intentional: first call verifies the employee exists (returns 404 if not),
        // second call fetches the history. A single query joining existence + history would return an
        // empty list for a missing employee, which is ambiguous.
        var employee = await _employeeRepository.GetByIdAsync(request.BusinessEntityId, cancellationToken);
        if (employee is null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.BusinessEntityId} not found.");
        }

        var history = await _employeeRepository.GetEmployeeDepartmentHistoryAsync(
            request.BusinessEntityId, cancellationToken);

        return _mapper.Map<IReadOnlyList<EmployeeDepartmentHistoryModel>>(history);
    }
}
