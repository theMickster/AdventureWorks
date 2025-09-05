using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadEmployeesByDepartmentQuery : IRequest<(IReadOnlyList<EmployeeModel> Employees, int TotalCount)>
{
    public required short DepartmentId { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
