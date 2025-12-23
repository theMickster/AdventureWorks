using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Query to retrieve a single vendor's profile and spend metrics.
/// </summary>
public sealed class ReadVendorDetailQuery : IRequest<VendorDetailModel>
{
    /// <summary>
    /// The vendor primary key.
    /// </summary>
    public required int VendorId { get; set; }
}
