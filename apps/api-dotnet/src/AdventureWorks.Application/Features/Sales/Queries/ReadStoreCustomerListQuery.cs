using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a paged list of customers associated with a single store,
/// enriched with per-customer order aggregates. Returns <c>null</c> when the
/// store does not exist so the controller can translate to a 404.
/// </summary>
public sealed class ReadStoreCustomerListQuery : IRequest<StoreCustomerSearchResultModel?>
{
    /// <summary>The store's business entity identifier.</summary>
    public required int StoreId { get; init; }

    /// <summary>Paging, sort column, and sort order parameters.</summary>
    public required StoreCustomerParameter Parameters { get; init; }
}
