using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Query to retrieve all phone numbers for a person.
/// </summary>
public sealed class ReadPersonPhoneListQuery : IRequest<List<PersonPhoneModel>>
{
    /// <summary>
    /// The person's BusinessEntityId.
    /// </summary>
    public required int PersonId { get; set; }
}
