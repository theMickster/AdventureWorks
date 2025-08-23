using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve combined standard cost and list price history for a specific product.
/// </summary>
public sealed class ReadProductPriceHistoryQuery : IRequest<List<ProductPriceHistoryModel>>
{
    /// <summary>The unique product identifier.</summary>
    public required int ProductId { get; set; }
}
