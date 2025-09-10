using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Commands;

/// <summary>
/// Handler for <see cref="UpdatePersonPhoneCommand"/>.
/// Validates person existence (404), validates the payload, locates the existing phone (404 if missing),
/// replaces the phone number via transactional delete+insert, then re-hydrates the entity before mapping.
/// Returns the updated <see cref="PersonPhoneModel"/>.
/// </summary>
public sealed class UpdatePersonPhoneCommandHandler(
    IMapper mapper,
    IPersonPhoneRepository personPhoneRepository,
    IValidator<PersonPhoneUpdateModel> validator)
        : IRequestHandler<UpdatePersonPhoneCommand, PersonPhoneModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonPhoneRepository _personPhoneRepository = personPhoneRepository ?? throw new ArgumentNullException(nameof(personPhoneRepository));
    private readonly IValidator<PersonPhoneUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<PersonPhoneModel> Handle(UpdatePersonPhoneCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        if (!await _personPhoneRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var existing = await _personPhoneRepository.GetTrackedPhoneAsync(
            request.PersonId, request.PhoneNumberTypeId, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"Phone number with type {request.PhoneNumberTypeId} not found for person {request.PersonId}.");
        }

        if (await _personPhoneRepository.PhoneCombinationExistsAsync(
            request.PersonId, request.Model.PhoneNumber, request.PhoneNumberTypeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(PersonPhoneUpdateModel.PhoneNumber), "This phone number and type combination already exists for this person.")
                {
                    ErrorCode = "Rule-04"
                }
            });
        }

        await _personPhoneRepository.ReplacePhoneAsync(existing, request.Model.PhoneNumber, DateTime.UtcNow, cancellationToken);

        var entityWithDetails = await _personPhoneRepository.GetPhoneWithDetailsByCompositeKeyAsync(
            request.PersonId, request.Model.PhoneNumber, request.PhoneNumberTypeId, cancellationToken);

        if (entityWithDetails is null)
        {
            throw new KeyNotFoundException(
                $"Phone number could not be retrieved after update for person {request.PersonId}.");
        }

        return _mapper.Map<PersonPhoneModel>(entityWithDetails);
    }
}
