using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Command to update an existing email address for a person.
/// </summary>
public sealed class UpdatePersonEmailCommand : IRequest<int>
{
    /// <summary>
    /// The person's BusinessEntityId (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// The email address identifier (route value).
    /// </summary>
    public int EmailAddressId { get; set; }

    /// <summary>
    /// The update payload.
    /// </summary>
    public required PersonEmailUpdateModel Model { get; set; }
}
