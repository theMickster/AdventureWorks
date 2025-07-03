using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Query to retrieve a paginated list of employees with optional search criteria.
/// </summary>
public sealed class ReadEmployeeListQuery : IRequest<EmployeeSearchResultModel>
{
    /// <summary>
    /// Pagination and sorting parameters.
    /// </summary>
    public required EmployeeParameter Parameters { get; set; }

    /// <summary>
    /// Optional search criteria to filter employees.
    /// </summary>
    public EmployeeSearchModel? SearchModel { get; set; }
}
