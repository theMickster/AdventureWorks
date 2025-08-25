using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Command to add a new contact (BusinessEntityContact) to a store.
/// </summary>
public sealed class AddStoreContactCommand : IRequest<int>
{
    /// <summary>
    /// Store's business entity identifier (route value).
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// The contact payload to add.
    /// </summary>
    public required StoreContactCreateModel Model { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// System-generated row guid.
    /// </summary>
    public Guid RowGuid { get; set; }
}
