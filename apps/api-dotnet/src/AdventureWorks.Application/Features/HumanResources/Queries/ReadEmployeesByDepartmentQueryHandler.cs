using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Handles <see cref="ReadEmployeesByDepartmentQuery"/> by returning a paginated list of active employees in a department.</summary>
public sealed class ReadEmployeesByDepartmentQueryHandler(
    IMapper mapper,
    IDepartmentRepository departmentRepository)
        : IRequestHandler<ReadEmployeesByDepartmentQuery, (IReadOnlyList<EmployeeModel> Employees, int TotalCount)>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDepartmentRepository _repository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    public async Task<(IReadOnlyList<EmployeeModel> Employees, int TotalCount)> Handle(ReadEmployeesByDepartmentQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dept = await _repository.GetByIdAsync(request.DepartmentId, cancellationToken) ?? throw new KeyNotFoundException($"Department with ID {request.DepartmentId} not found.");
        var (employees, totalCount) = await _repository.GetEmployeesByDepartmentAsync(
            request.DepartmentId, request.Page, request.PageSize, cancellationToken);

        return (_mapper.Map<IReadOnlyList<EmployeeModel>>(employees), totalCount);
    }
}
