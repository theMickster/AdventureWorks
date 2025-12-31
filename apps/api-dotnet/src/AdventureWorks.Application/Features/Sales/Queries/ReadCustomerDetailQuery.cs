using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a single customer's full detail, including LTV rank and spend metrics.
/// </summary>
public sealed class ReadCustomerDetailQuery : IRequest<CustomerDetailModel>
{
    /// <summary>
    /// The customer primary key.
    /// </summary>
    public required int CustomerId { get; set; }
}
