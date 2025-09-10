using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Handler for <see cref="ReadPersonPhoneListQuery"/>.
/// Returns the list of phone numbers for the person, or an empty list when the person has no phones.
/// Throws <see cref="KeyNotFoundException"/> when the person does not exist (produces 404).
/// </summary>
public sealed class ReadPersonPhoneListQueryHandler(
    IMapper mapper,
    IPersonPhoneRepository personPhoneRepository)
        : IRequestHandler<ReadPersonPhoneListQuery, List<PersonPhoneModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonPhoneRepository _personPhoneRepository = personPhoneRepository ?? throw new ArgumentNullException(nameof(personPhoneRepository));

    public async Task<List<PersonPhoneModel>> Handle(ReadPersonPhoneListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _personPhoneRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        var phones = await _personPhoneRepository.GetPhonesByPersonIdAsync(request.PersonId, cancellationToken);

        if (phones is null or { Count: 0 })
        {
            return [];
        }

        return _mapper.Map<List<PersonPhoneModel>>(phones);
    }
}
