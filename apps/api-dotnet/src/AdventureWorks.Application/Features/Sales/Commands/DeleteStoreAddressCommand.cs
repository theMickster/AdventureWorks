using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Command to delete a store address identified by its composite key.
/// </summary>
public sealed class DeleteStoreAddressCommand : IRequest<Unit>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Address identifier.
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// Address type identifier.
    /// </summary>
    public int AddressTypeId { get; set; }
}
