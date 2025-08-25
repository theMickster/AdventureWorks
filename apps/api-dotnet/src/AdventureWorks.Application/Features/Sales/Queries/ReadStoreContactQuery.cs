using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve a single store contact by its composite key (StoreId + PersonId + ContactTypeId).
/// </summary>
public sealed class ReadStoreContactQuery : IRequest<StoreContactModel?>
{
    /// <summary>
    /// Store's business entity identifier.
    /// </summary>
    public required int StoreId { get; init; }

    /// <summary>
    /// Person identifier.
    /// </summary>
    public required int PersonId { get; init; }

    /// <summary>
    /// Contact type identifier.
    /// </summary>
    public required int ContactTypeId { get; init; }
}
