using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a single store address by its composite key (StoreId + AddressId + AddressTypeId).
/// </summary>
public sealed class ReadStoreAddressQuery : IRequest<StoreAddressModel?>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; init; }

    /// <summary>
    /// Address identifier.
    /// </summary>
    public required int AddressId { get; init; }

    /// <summary>
    /// Address type identifier.
    /// </summary>
    public required int AddressTypeId { get; init; }
}
