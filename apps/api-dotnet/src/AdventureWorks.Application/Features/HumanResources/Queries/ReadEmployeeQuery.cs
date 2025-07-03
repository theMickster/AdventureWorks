using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Query to retrieve an employee by their business entity ID.
/// </summary>
public sealed class ReadEmployeeQuery : IRequest<EmployeeModel?>
{
    /// <summary>
    /// Business entity identifier of the employee to retrieve.
    /// </summary>
    public required int BusinessEntityId { get; set; } = int.MinValue;
}
