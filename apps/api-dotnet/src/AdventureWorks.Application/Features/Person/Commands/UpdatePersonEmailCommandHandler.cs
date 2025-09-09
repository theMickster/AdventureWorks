using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Handler for <see cref="UpdatePersonEmailCommand"/>.
/// Validates person existence (404), validates the payload, locates the existing email (404 if missing),
/// enforces duplicate email uniqueness (Rule-03 when the address changes), and persists the update.
/// </summary>
public sealed class UpdatePersonEmailCommandHandler(
    IPersonEmailRepository personEmailRepository,
    IValidator<PersonEmailUpdateModel> validator)
        : IRequestHandler<UpdatePersonEmailCommand, int>
{
    private readonly IPersonEmailRepository _personEmailRepository = personEmailRepository ?? throw new ArgumentNullException(nameof(personEmailRepository));
    private readonly IValidator<PersonEmailUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(UpdatePersonEmailCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        if (!await _personEmailRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var existing = await _personEmailRepository.GetEmailByCompositeKeyAsync(
            request.PersonId, request.EmailAddressId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Email address with ID {request.EmailAddressId} not found for person {request.PersonId}.");
        }

        if (!string.Equals(existing.EmailAddressName, request.Model.EmailAddress, StringComparison.OrdinalIgnoreCase))
        {
            if (await _personEmailRepository.EmailExistsForPersonAsync(request.PersonId, request.Model.EmailAddress, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(PersonEmailUpdateModel.EmailAddress), MessageDuplicateEmail)
                    {
                        ErrorCode = "Rule-03"
                    }
                });
            }
        }

        existing.EmailAddressName = request.Model.EmailAddress;
        existing.ModifiedDate = DateTime.UtcNow;

        await _personEmailRepository.UpdateAsync(existing, cancellationToken);

        return request.EmailAddressId;
    }

    public static string MessageDuplicateEmail => "This email address already exists for the person.";
}
