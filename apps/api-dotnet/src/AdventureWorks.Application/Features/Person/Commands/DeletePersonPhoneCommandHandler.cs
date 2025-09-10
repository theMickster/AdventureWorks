using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Handler for <see cref="DeletePersonPhoneCommand"/>.
/// Validates person existence (404), locates the existing phone (404 if missing), and hard-deletes it.
/// </summary>
public sealed class DeletePersonPhoneCommandHandler(
    IPersonPhoneRepository personPhoneRepository)
        : IRequestHandler<DeletePersonPhoneCommand, Unit>
{
    private readonly IPersonPhoneRepository _personPhoneRepository = personPhoneRepository ?? throw new ArgumentNullException(nameof(personPhoneRepository));

    public async Task<Unit> Handle(DeletePersonPhoneCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _personPhoneRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        var existing = await _personPhoneRepository.GetTrackedPhoneAsync(
            request.PersonId, request.PhoneNumberTypeId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Phone number with type {request.PhoneNumberTypeId} not found for person {request.PersonId}.");
        }

        await _personPhoneRepository.DeletePhoneAsync(request.PersonId, request.PhoneNumberTypeId, cancellationToken);

        return Unit.Value;
    }
}
