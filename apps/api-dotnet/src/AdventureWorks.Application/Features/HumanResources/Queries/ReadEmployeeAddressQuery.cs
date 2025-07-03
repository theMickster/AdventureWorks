using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Query to retrieve a specific address for an employee.
/// </summary>
public sealed class ReadEmployeeAddressQuery : IRequest<EmployeeAddressModel?>
{
    /// <summary>
    /// Employee's business entity identifier.
    /// </summary>
    public required int BusinessEntityId { get; set; }

    /// <summary>
    /// Address identifier.
    /// </summary>
    public required int AddressId { get; set; }
}
