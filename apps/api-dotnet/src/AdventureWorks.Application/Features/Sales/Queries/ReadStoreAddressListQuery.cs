using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve all addresses for a store.
/// </summary>
public sealed class ReadStoreAddressListQuery : IRequest<List<StoreAddressModel>>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; set; }
}
