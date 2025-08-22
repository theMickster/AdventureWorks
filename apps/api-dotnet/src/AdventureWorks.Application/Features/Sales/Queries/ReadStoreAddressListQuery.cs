using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve all addresses for a store.
/// </summary>
public sealed class ReadStoreAddressListQuery : IRequest<List<BusinessEntityAddressModel>>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; set; }
}
