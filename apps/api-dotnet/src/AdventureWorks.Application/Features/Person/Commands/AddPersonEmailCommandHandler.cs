using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Handler for <see cref="AddPersonEmailCommand"/>.
/// Validates person existence (404), validates the payload (Rule-01 required, Rule-02 format, Rule-04 length),
/// enforces duplicate email uniqueness (Rule-03), and persists the new email address.
/// Returns the saved <see cref="PersonEmailModel"/> including the DB-generated EmailAddressId.
/// </summary>
public sealed class AddPersonEmailCommandHandler(
    IMapper mapper,
    IPersonEmailRepository personEmailRepository,
    IValidator<PersonEmailCreateModel> validator)
        : IRequestHandler<AddPersonEmailCommand, PersonEmailModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonEmailRepository _personEmailRepository = personEmailRepository ?? throw new ArgumentNullException(nameof(personEmailRepository));
    private readonly IValidator<PersonEmailCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<PersonEmailModel> Handle(AddPersonEmailCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        if (!await _personEmailRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        if (await _personEmailRepository.EmailExistsForPersonAsync(request.PersonId, request.Model.EmailAddress, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(PersonEmailCreateModel.EmailAddress), MessageDuplicateEmail)
                {
                    ErrorCode = "Rule-03"
                }
            });
        }

        var entity = _mapper.Map<EmailAddressEntity>(request.Model);
        entity.BusinessEntityId = request.PersonId;
        entity.Rowguid = Guid.NewGuid();
        entity.ModifiedDate = DateTime.UtcNow;

        await _personEmailRepository.AddAsync(entity, cancellationToken);

        return _mapper.Map<PersonEmailModel>(entity);
    }

    public static string MessageDuplicateEmail => "This email address already exists for the person.";
}
