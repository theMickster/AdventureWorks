using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Handler for <see cref="DeletePersonEmailCommand"/>.
/// Locates the email by composite key and hard-deletes it. Throws <see cref="KeyNotFoundException"/> if missing.
/// </summary>
public sealed class DeletePersonEmailCommandHandler(
    IPersonEmailRepository personEmailRepository)
        : IRequestHandler<DeletePersonEmailCommand, Unit>
{
    private readonly IPersonEmailRepository _personEmailRepository = personEmailRepository ?? throw new ArgumentNullException(nameof(personEmailRepository));

    public async Task<Unit> Handle(DeletePersonEmailCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _personEmailRepository.GetEmailByCompositeKeyAsync(
            request.PersonId, request.EmailAddressId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Email address with ID {request.EmailAddressId} not found for person {request.PersonId}.");
        }

        await _personEmailRepository.DeleteEmailAsync(request.PersonId, request.EmailAddressId, cancellationToken);

        return Unit.Value;
    }
}
