using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Command to delete a phone number from a person.
/// </summary>
public sealed class DeletePersonPhoneCommand : IRequest<Unit>
{
    /// <summary>
    /// The person's BusinessEntityId (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// The phone number type identifier (route value).
    /// </summary>
    public int PhoneNumberTypeId { get; set; }
}
