using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Command to add a new email address to a person.
/// </summary>
public sealed class AddPersonEmailCommand : IRequest<PersonEmailModel>
{
    /// <summary>
    /// The person's BusinessEntityId (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// The email address payload.
    /// </summary>
    public required PersonEmailCreateModel Model { get; set; }
}
