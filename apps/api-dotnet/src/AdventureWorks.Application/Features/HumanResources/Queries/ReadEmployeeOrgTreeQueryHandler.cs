using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadEmployeeOrgTreeQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<ReadEmployeeOrgTreeQuery, IReadOnlyList<EmployeeOrgTreeItemModel>>
{
    private readonly IEmployeeRepository _employeeRepository =
        employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public Task<IReadOnlyList<EmployeeOrgTreeItemModel>> Handle(
        ReadEmployeeOrgTreeQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _employeeRepository.GetOrgTreeAsync(cancellationToken);
    }
}
