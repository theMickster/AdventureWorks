using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve all contacts for a store.
/// </summary>
public sealed class ReadStoreContactListQuery : IRequest<List<StoreContactModel>>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; set; }
}
