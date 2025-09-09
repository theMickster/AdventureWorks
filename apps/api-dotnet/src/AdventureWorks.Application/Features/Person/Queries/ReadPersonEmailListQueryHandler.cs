using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Handler for <see cref="ReadPersonEmailListQuery"/>.
/// Returns the list of email addresses for the person, or an empty list when the person has no emails.
/// Throws <see cref="KeyNotFoundException"/> when the person does not exist (produces 404).
/// </summary>
public sealed class ReadPersonEmailListQueryHandler(
    IMapper mapper,
    IPersonEmailRepository personEmailRepository)
        : IRequestHandler<ReadPersonEmailListQuery, List<PersonEmailModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonEmailRepository _personEmailRepository = personEmailRepository ?? throw new ArgumentNullException(nameof(personEmailRepository));

    public async Task<List<PersonEmailModel>> Handle(ReadPersonEmailListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _personEmailRepository.PersonExistsAsync(request.PersonId, cancellationToken))
        {
            throw new KeyNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        var emails = await _personEmailRepository.GetEmailsByPersonIdAsync(request.PersonId, cancellationToken);

        if (emails is null or { Count: 0 })
        {
            return [];
        }

        return _mapper.Map<List<PersonEmailModel>>(emails);
    }
}
