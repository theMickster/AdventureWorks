using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a single store's parsed demographics survey.
/// </summary>
public sealed class ReadStoreDemographicsQuery : IRequest<StoreDemographicsModel?>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; init; }
}
