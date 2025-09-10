using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Command to update the phone number for an existing person phone record.
/// The phone type stays fixed; only the number changes (transactional delete+insert in repository).
/// </summary>
public sealed class UpdatePersonPhoneCommand : IRequest<PersonPhoneModel>
{
    /// <summary>
    /// The person's BusinessEntityId (route value).
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// The phone number type identifier (route value) — identifies which phone to replace.
    /// </summary>
    public int PhoneNumberTypeId { get; set; }

    /// <summary>
    /// The update payload containing the new phone number.
    /// </summary>
    public required PersonPhoneUpdateModel Model { get; set; }
}
