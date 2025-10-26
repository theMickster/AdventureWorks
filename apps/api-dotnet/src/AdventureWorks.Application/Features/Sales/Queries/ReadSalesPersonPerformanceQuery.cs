using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve performance metrics for a single sales person.
/// </summary>
public sealed class ReadSalesPersonPerformanceQuery : IRequest<SalesPersonPerformanceModel?>
{
    /// <summary>Gets or sets the sales person's <c>BusinessEntityId</c>.</summary>
    public required int Id { get; set; } = int.MinValue;
}
