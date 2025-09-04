using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Query to retrieve an employee's complete department assignment history.</summary>
public sealed class ReadEmployeeDepartmentHistoryQuery : IRequest<IReadOnlyList<EmployeeDepartmentHistoryModel>>
{
    /// <summary>The employee's business entity identifier.</summary>
    public int BusinessEntityId { get; set; }
}
