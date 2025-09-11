using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Handler for <see cref="ReadPersonQuery"/>.
/// </summary>
public sealed class ReadPersonQueryHandler(
    IMapper mapper,
    IPersonRepository personRepository)
        : IRequestHandler<ReadPersonQuery, PersonDetailModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonRepository _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));

    public async Task<PersonDetailModel?> Handle(ReadPersonQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var personEntity = await _personRepository.GetPersonDetailByIdAsync(request.PersonId, cancellationToken);

        return personEntity is null ? null : _mapper.Map<PersonDetailModel>(personEntity);
    }
}
