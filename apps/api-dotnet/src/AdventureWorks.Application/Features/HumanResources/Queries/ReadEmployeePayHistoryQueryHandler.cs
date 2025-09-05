using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Handles <see cref="ReadEmployeePayHistoryQuery"/> by returning an employee's complete pay history.</summary>
public sealed class ReadEmployeePayHistoryQueryHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository)
    : IRequestHandler<ReadEmployeePayHistoryQuery, IReadOnlyList<EmployeePayHistoryModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _employeeRepository =
        employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public async Task<IReadOnlyList<EmployeePayHistoryModel>> Handle(
        ReadEmployeePayHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var employee = await _employeeRepository.GetEmployeeByIdAsync(request.BusinessEntityId, cancellationToken);
        if (employee is null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.BusinessEntityId} not found.");
        }

        var history = await _employeeRepository.GetEmployeePayHistoryAsync(request.BusinessEntityId, cancellationToken);

        return _mapper.Map<IReadOnlyList<EmployeePayHistoryModel>>(history);
    }
}
