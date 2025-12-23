using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Handler for retrieving a vendor's paginated, filterable purchase order history.
/// </summary>
/// <remarks>
/// Throws <see cref="KeyNotFoundException"/> when the vendor does not exist at all — see
/// <see cref="ReadVendorDetailQueryHandler"/> for why this feature deviates from the
/// return-null-for-the-controller convention used elsewhere. A vendor that exists but has zero
/// purchase orders (or zero matching the filter) is a normal 200 with an empty page, distinguished
/// via the repository's <c>VendorExists</c> flag.
/// </remarks>
public sealed class ReadVendorPurchaseOrdersQueryHandler(
    IVendorRepository vendorRepository,
    IValidator<ReadVendorPurchaseOrdersQuery> validator)
    : IRequestHandler<ReadVendorPurchaseOrdersQuery, PurchaseOrderSearchResultModel>
{
    private readonly IVendorRepository _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
    private readonly IValidator<ReadVendorPurchaseOrdersQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the query to retrieve a vendor's paginated, filterable purchase order history.
    /// </summary>
    /// <param name="request">the query request containing the vendor identifier and filter/paging parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A search result model containing the paginated purchase order history</returns>
    /// <exception cref="KeyNotFoundException">no vendor with <see cref="ReadVendorPurchaseOrdersQuery.VendorId"/> exists</exception>
    public async Task<PurchaseOrderSearchResultModel> Handle(
        ReadVendorPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var (orders, totalCount, vendorExists) = await _vendorRepository.GetVendorPurchaseOrdersAsync(
            request.VendorId, request.Parameters, cancellationToken);

        if (!vendorExists)
        {
            throw new KeyNotFoundException($"Vendor {request.VendorId} was not found.");
        }

        return new PurchaseOrderSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = totalCount,
            Results = orders.ToList()
        };
    }
}
