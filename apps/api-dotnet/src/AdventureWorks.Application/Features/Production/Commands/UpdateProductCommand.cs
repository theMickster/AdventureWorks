using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Commands;

/// <summary>
/// Command to update an existing product. Returns no value on success.
/// </summary>
public sealed class UpdateProductCommand : IRequest
{
    /// <summary>The updated product data submitted by the caller. Must include the product ID.</summary>
    public required ProductUpdateModel Model { get; set; }

    /// <summary>Audit timestamp set by the controller before dispatching (UTC).</summary>
    public DateTime ModifiedDate { get; set; }
}
