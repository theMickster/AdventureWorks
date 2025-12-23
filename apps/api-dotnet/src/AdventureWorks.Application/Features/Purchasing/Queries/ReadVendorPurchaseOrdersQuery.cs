using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Query to retrieve a paginated, filterable page of a single vendor's purchase order history.
/// </summary>
public sealed class ReadVendorPurchaseOrdersQuery : IRequest<PurchaseOrderSearchResultModel>
{
    /// <summary>
    /// The vendor primary key.
    /// </summary>
    public required int VendorId { get; set; }

    /// <summary>
    /// Pagination and filtering parameters.
    /// </summary>
    public required VendorPurchaseOrderParameter Parameters { get; set; }
}
