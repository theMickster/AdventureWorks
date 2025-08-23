using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Commands;

/// <summary>
/// Command to create a new product. Returns the new product's ID on success.
/// </summary>
public sealed class CreateProductCommand : IRequest<int>
{
    /// <summary>The product data submitted by the caller.</summary>
    public required ProductCreateModel Model { get; set; }

    /// <summary>Audit timestamp set by the controller before dispatching (UTC).</summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>Uniqueness key set by the controller before dispatching.</summary>
    public Guid RowGuid { get; set; }
}
