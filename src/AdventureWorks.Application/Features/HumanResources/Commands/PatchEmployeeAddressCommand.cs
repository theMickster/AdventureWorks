using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to partially update an employee's address using JSON Patch (RFC 6902).
/// Updates only the Address table fields specified in the patch document.
/// </summary>
public sealed class PatchEmployeeAddressCommand : IRequest<Unit>
{
    /// <summary>
    /// Employee's business entity ID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Address ID to patch.
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// JSON Patch document containing the operations to apply.
    /// </summary>
    public required JsonPatchDocument<EmployeeAddressUpdateModel> PatchDocument { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
