using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to partially update an employee's personal information using JSON Patch (RFC 6902).
/// Updates PersonEntity and EmployeeEntity fields specified in the patch document.
/// The Id field is immutable and cannot be patched.
/// </summary>
public sealed class PatchEmployeeCommand : IRequest<Unit>
{
    /// <summary>
    /// Employee's business entity ID (immutable).
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// JSON Patch document containing the operations to apply.
    /// </summary>
    public required JsonPatchDocument<EmployeeUpdateModel> PatchDocument { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
