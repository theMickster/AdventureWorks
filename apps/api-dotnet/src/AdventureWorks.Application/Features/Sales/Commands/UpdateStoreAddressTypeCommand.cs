using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Command to change the address type of an existing store address.
/// Because the composite primary key includes <c>AddressTypeId</c>, this is implemented as a delete-and-insert.
/// </summary>
public sealed class UpdateStoreAddressTypeCommand : IRequest<int>
{
    /// <summary>
    /// Store's business entity identifier (route value).
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Address identifier (route value).
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// Existing address type identifier (route value) - the row to be replaced.
    /// </summary>
    public int CurrentAddressTypeId { get; set; }

    /// <summary>
    /// The update payload (carries the target AddressTypeId).
    /// </summary>
    public required StoreAddressUpdateModel Model { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
