using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve a filtered, paginated list of products using the supplied search criteria.
/// </summary>
public sealed class SearchProductsQuery : IRequest<ProductSearchResultModel>
{
    /// <summary>Paging and sort parameters (page number, page size, order by, sort direction).</summary>
    public required ProductParameter Parameters { get; set; }

    /// <summary>Filter criteria applied to the product set (name, category, price range, etc.).</summary>
    public required ProductSearchModel SearchModel { get; set; }
}
