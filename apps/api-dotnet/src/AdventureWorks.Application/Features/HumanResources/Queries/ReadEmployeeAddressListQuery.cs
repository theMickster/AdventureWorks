using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Query to retrieve all addresses for an employee.
/// </summary>
public sealed class ReadEmployeeAddressListQuery : IRequest<IReadOnlyList<EmployeeAddressModel>>
{
    /// <summary>
    /// Employee's business entity identifier.
    /// </summary>
    public required int BusinessEntityId { get; set; }
}
