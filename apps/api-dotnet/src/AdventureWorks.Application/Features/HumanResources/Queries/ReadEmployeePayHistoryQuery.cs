using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Query to retrieve an employee's complete pay history.</summary>
public sealed class ReadEmployeePayHistoryQuery : IRequest<IReadOnlyList<EmployeePayHistoryModel>>
{
    /// <summary>The employee's business entity identifier.</summary>
    public required int BusinessEntityId { get; init; }
}
