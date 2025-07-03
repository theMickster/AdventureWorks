using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Handler for retrieving a paginated list of employees with optional search criteria.
/// </summary>
public sealed class ReadEmployeeListQueryHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository)
        : IRequestHandler<ReadEmployeeListQuery, EmployeeSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public async Task<EmployeeSearchResultModel> Handle(ReadEmployeeListQuery request, CancellationToken cancellationToken)
    {
        var result = new EmployeeSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0
        };

        IReadOnlyList<EmployeeEntity> employeeEntities;
        var totalRecords = 0;

        if (request.SearchModel is null)
        {
            (employeeEntities, totalRecords) = await _employeeRepository.GetEmployeesAsync(request.Parameters, cancellationToken);
        }
        else
        {
            (employeeEntities, totalRecords) = await _employeeRepository.SearchEmployeesAsync(request.Parameters, request.SearchModel, cancellationToken);
        }

        if (employeeEntities is null or { Count: 0 })
        {
            return result;
        }

        var employees = _mapper.Map<List<EmployeeModel>>(employeeEntities);

        result.Results = employees;
        result.TotalRecords = totalRecords;

        return result;
    }
}
