using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Command to add a new phone number to a person.
/// </summary>
public sealed class AddPersonPhoneCommand : IRequest<PersonPhoneModel>
{
    /// <summary>
    /// The person's BusinessEntityId (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// The phone number payload.
    /// </summary>
    public required PersonPhoneCreateModel Model { get; set; }
}
