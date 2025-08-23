using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve product subcategories, optionally filtered by parent category.
/// </summary>
public sealed class ReadProductSubcategoriesQuery : IRequest<List<ProductSubcategoryModel>>
{
    /// <summary>When provided, limits results to subcategories belonging to this category ID.</summary>
    public int? CategoryId { get; set; }
}
