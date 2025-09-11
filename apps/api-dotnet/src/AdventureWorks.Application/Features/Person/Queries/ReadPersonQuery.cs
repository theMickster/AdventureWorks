using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Query to retrieve consolidated details for a single person.
/// </summary>
public sealed class ReadPersonQuery : IRequest<PersonDetailModel?>
{
    /// <summary>
    /// The person's BusinessEntityId.
    /// </summary>
    public required int PersonId { get; set; }
}
