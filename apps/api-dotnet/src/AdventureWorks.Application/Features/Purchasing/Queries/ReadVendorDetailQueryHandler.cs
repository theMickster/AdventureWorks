using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Handler for retrieving a single vendor's profile and spend metrics.
/// </summary>
/// <remarks>
/// Throws <see cref="KeyNotFoundException"/> — caught by <c>ExceptionHandlerMiddleware</c> and translated
/// to a structured 404 body (<c>error</c>/<c>correlationId</c>/<c>timestamp</c>) — rather than returning
/// null for the controller to translate. This follows the dominant pattern for sub-resource/parent-existence
/// read failures elsewhere in the codebase (e.g. <see cref="AdventureWorks.Application.Features.Person.Queries.ReadPersonPhoneListQueryHandler"/>,
/// <see cref="AdventureWorks.Application.Features.HumanResources.Queries.ReadDepartmentHeadcountQueryHandler"/>),
/// as distinct from single-entity detail reads (e.g. <see cref="ReadSalesOrderDetailQueryHandler"/>), which
/// return null for the controller to translate. <c>GET /vendors/{id}</c> deliberately takes the throw side
/// of that split to get a structured error body.
/// </remarks>
public sealed class ReadVendorDetailQueryHandler(IVendorRepository vendorRepository)
    : IRequestHandler<ReadVendorDetailQuery, VendorDetailModel>
{
    private readonly IVendorRepository _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));

    /// <summary>
    /// Handles the query to retrieve a single vendor's profile and spend metrics.
    /// </summary>
    /// <param name="request">the query request containing the vendor identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The vendor detail</returns>
    /// <exception cref="KeyNotFoundException">no vendor with <see cref="ReadVendorDetailQuery.VendorId"/> exists</exception>
    public async Task<VendorDetailModel> Handle(ReadVendorDetailQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var detail = await _vendorRepository.GetVendorDetailAsync(request.VendorId, cancellationToken);

        return detail ?? throw new KeyNotFoundException($"Vendor {request.VendorId} was not found.");
    }
}
