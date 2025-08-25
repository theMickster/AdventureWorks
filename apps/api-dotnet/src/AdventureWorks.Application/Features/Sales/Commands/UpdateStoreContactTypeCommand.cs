using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Command to change the contact type of an existing store contact.
/// Because the composite primary key includes <c>ContactTypeId</c>, this is implemented as a delete-and-insert.
/// </summary>
public sealed class UpdateStoreContactTypeCommand : IRequest<int>
{
    /// <summary>
    /// Store's business entity identifier (route value).
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Person identifier (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// Existing contact type identifier (route value) - the row to be replaced.
    /// </summary>
    public int CurrentContactTypeId { get; set; }

    /// <summary>
    /// The update payload (carries the target ContactTypeId).
    /// </summary>
    public required StoreContactUpdateModel Model { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
