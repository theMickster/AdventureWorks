using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Command to delete an email address from a person.
/// </summary>
public sealed class DeletePersonEmailCommand : IRequest<Unit>
{
    /// <summary>
    /// The person's BusinessEntityId (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// The email address identifier (route value).
    /// </summary>
    public int EmailAddressId { get; set; }
}
