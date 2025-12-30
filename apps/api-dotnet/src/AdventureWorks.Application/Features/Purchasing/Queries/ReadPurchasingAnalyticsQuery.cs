using AdventureWorks.Models.Features.Purchasing;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Query that returns the aggregate purchasing analytics: Pareto vendor-spend data and the
/// purchase order pipeline summary.
/// </summary>
public sealed class ReadPurchasingAnalyticsQuery : IRequest<PurchasingAnalyticsModel> { }
