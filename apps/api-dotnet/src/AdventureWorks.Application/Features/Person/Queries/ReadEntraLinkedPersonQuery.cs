using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Query to retrieve a Person entity linked to a Microsoft Entra ID user.
/// </summary>
public sealed class ReadEntraLinkedPersonQuery : IRequest<EntraLinkedPersonModel?>
{
    /// <summary>
    /// The Microsoft Entra Object ID (oid claim) to search for.
    /// </summary>
    public required Guid EntraObjectId { get; set; }
}
