using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a single store's calendar-year sales performance summary.
/// </summary>
public sealed class ReadStorePerformanceQuery : IRequest<StorePerformanceModel?>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; init; }
}
