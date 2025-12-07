using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a paged, LTV-ranked list of customers.
/// </summary>
public sealed class ReadCustomerListQuery : IRequest<CustomerSearchResultModel>
{
    /// <summary>Paging and search parameters.</summary>
    public required CustomerParameter Parameters { get; init; }
}
