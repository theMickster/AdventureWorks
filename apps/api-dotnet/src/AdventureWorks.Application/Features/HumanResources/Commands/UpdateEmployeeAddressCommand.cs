using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to update an employee's address information.
/// Updates only the Address table fields, not the BusinessEntityAddress junction.
/// </summary>
public sealed class UpdateEmployeeAddressCommand : IRequest<Unit>
{
    /// <summary>
    /// Employee's business entity ID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Address update data.
    /// </summary>
    public required EmployeeAddressUpdateModel Model { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
