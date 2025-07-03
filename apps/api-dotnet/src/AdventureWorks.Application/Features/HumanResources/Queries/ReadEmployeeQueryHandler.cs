using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Handler for retrieving an employee by their business entity ID.
/// </summary>
public sealed class ReadEmployeeQueryHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository)
        : IRequestHandler<ReadEmployeeQuery, EmployeeModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _repository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public async Task<EmployeeModel?> Handle(ReadEmployeeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var employeeEntity = await _repository.GetEmployeeByIdAsync(request.BusinessEntityId, cancellationToken);

        if (employeeEntity == null)
        {
            return null;
        }

        return _mapper.Map<EmployeeModel>(employeeEntity);
    }
}
