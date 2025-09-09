using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Handler for <see cref="ReadPersonEmailQuery"/>.
/// Returns null when the email does not exist; the controller translates null to 404.
/// </summary>
public sealed class ReadPersonEmailQueryHandler(
    IMapper mapper,
    IPersonEmailRepository personEmailRepository)
        : IRequestHandler<ReadPersonEmailQuery, PersonEmailModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonEmailRepository _personEmailRepository = personEmailRepository ?? throw new ArgumentNullException(nameof(personEmailRepository));

    public async Task<PersonEmailModel?> Handle(ReadPersonEmailQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _personEmailRepository.GetEmailByCompositeKeyAsync(
            request.PersonId, request.EmailAddressId, cancellationToken);

        return entity is null ? null : _mapper.Map<PersonEmailModel>(entity);
    }
}
