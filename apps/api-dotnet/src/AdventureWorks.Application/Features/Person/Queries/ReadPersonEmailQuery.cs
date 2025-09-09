using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Query to retrieve a single email address by composite key (PersonId + EmailAddressId).
/// </summary>
public sealed class ReadPersonEmailQuery : IRequest<PersonEmailModel?>
{
    /// <summary>
    /// The person's BusinessEntityId.
    /// </summary>
    public required int PersonId { get; set; }

    /// <summary>
    /// The email address identifier.
    /// </summary>
    public required int EmailAddressId { get; set; }
}
