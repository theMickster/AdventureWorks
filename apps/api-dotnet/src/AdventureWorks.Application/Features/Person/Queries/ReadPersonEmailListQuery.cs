using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Query to retrieve all email addresses for a person.
/// </summary>
public sealed class ReadPersonEmailListQuery : IRequest<List<PersonEmailModel>>
{
    /// <summary>
    /// The person's BusinessEntityId.
    /// </summary>
    public required int PersonId { get; set; }
}
