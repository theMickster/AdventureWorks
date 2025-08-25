using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Command to delete a store contact identified by its composite key.
/// </summary>
public sealed class DeleteStoreContactCommand : IRequest<Unit>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Person identifier.
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// Contact type identifier.
    /// </summary>
    public int ContactTypeId { get; set; }
}
