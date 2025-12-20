using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Query to retrieve a paginated, risk-ranked list of vendors.
/// </summary>
public sealed class ReadVendorListQuery : IRequest<VendorSearchResultModel>
{
    /// <summary>
    /// Pagination and filtering parameters.
    /// </summary>
    public required VendorParameter Parameters { get; set; }
}
