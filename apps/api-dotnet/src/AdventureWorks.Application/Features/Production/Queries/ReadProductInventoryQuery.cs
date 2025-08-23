using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve stock-on-hand by warehouse location for a specific product.
/// </summary>
public sealed class ReadProductInventoryQuery : IRequest<List<ProductInventoryModel>>
{
    /// <summary>The unique product identifier.</summary>
    public required int ProductId { get; set; }
}
