using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve a single product by ID with full navigation data
/// (category, subcategory, model, photos, inventory summary).
/// </summary>
public sealed class ReadProductQuery : IRequest<ProductDetailModel?>
{
    /// <summary>The unique product identifier.</summary>
    public required int Id { get; set; }
}
