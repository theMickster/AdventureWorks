using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Handler for <see cref="AddPersonPhoneCommand"/>.
/// Validates person existence (404), validates the payload (Rule-01, Rule-02),
/// enforces phone number type existence (Rule-03) and duplicate combo uniqueness (Rule-04),
/// and persists the new phone number.
/// Returns the saved <see cref="PersonPhoneModel"/> including the PhoneNumberType nav property.
/// </summary>
public sealed class AddPersonPhoneCommandHandler(
    IMapper mapper,
    IPersonPhoneRepository personPhoneRepository,
    IValidator<PersonPhoneCreateModel> validator)
        : IRequestHandler<AddPersonPhoneCommand, PersonPhoneModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonPhoneRepository _personPhoneRepository = personPhoneRepository ?? throw new ArgumentNullException(nameof(personPhoneRepository));
    private readonly IValidator<PersonPhoneCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<PersonPhoneModel> Handle(AddPersonPhoneCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        if (!await _personPhoneRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        if (!await _personPhoneRepository.PhoneNumberTypeExistsAsync(request.Model.PhoneNumberTypeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(PersonPhoneCreateModel.PhoneNumberTypeId), MessagePhoneNumberTypeNotFound)
                {
                    ErrorCode = "Rule-03"
                }
            });
        }

        if (await _personPhoneRepository.PhoneCombinationExistsAsync(request.PersonId, request.Model.PhoneNumber, request.Model.PhoneNumberTypeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(PersonPhoneCreateModel.PhoneNumber), MessageDuplicatePhoneCombination)
                {
                    ErrorCode = "Rule-04"
                }
            });
        }

        var entity = _mapper.Map<PersonPhone>(request.Model);
        entity.BusinessEntityId = request.PersonId;
        entity.ModifiedDate = DateTime.UtcNow;

        await _personPhoneRepository.AddAsync(entity, cancellationToken);

        var entityWithDetails = await _personPhoneRepository.GetPhoneWithDetailsByCompositeKeyAsync(
            request.PersonId, entity.PhoneNumber, entity.PhoneNumberTypeId, cancellationToken);

        if (entityWithDetails is null)
        {
            throw new KeyNotFoundException(
                $"Phone number could not be retrieved after creation for person {request.PersonId}.");
        }

        return _mapper.Map<PersonPhoneModel>(entityWithDetails);
    }

    public static string MessagePhoneNumberTypeNotFound => "Phone number type not found.";
    public static string MessageDuplicatePhoneCombination => "This phone number and type combination already exists for this person.";
}
