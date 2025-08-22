using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Command to partially update a store using JSON Patch (RFC 6902).
/// The StoreId field is immutable and cannot be patched.
/// </summary>
public sealed class PatchStoreCommand : IRequest<Unit>
{
    /// <summary>
    /// Store's business entity ID (immutable).
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// JSON Patch document containing the operations to apply.
    /// </summary>
    public required JsonPatchDocument<StoreUpdateModel> PatchDocument { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
