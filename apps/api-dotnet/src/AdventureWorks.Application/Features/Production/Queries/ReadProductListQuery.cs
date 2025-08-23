using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve a paginated, optionally sorted list of all products.
/// </summary>
public sealed class ReadProductListQuery : IRequest<ProductSearchResultModel>
{
    /// <summary>Paging and sort parameters (page number, page size, order by, sort direction).</summary>
    public required ProductParameter Parameters { get; set; }
}
