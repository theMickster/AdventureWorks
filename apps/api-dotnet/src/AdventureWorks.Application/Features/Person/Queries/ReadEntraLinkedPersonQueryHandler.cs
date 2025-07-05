using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Handler for ReadEntraLinkedPersonQuery.
/// Retrieves and validates a Person entity linked to a Microsoft Entra user.
/// </summary>
public sealed class ReadEntraLinkedPersonQueryHandler(
    IMapper mapper,
    IPersonRepository personRepository,
    ILogger<ReadEntraLinkedPersonQueryHandler> logger)
        : IRequestHandler<ReadEntraLinkedPersonQuery, EntraLinkedPersonModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonRepository _repository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    private readonly ILogger<ReadEntraLinkedPersonQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<EntraLinkedPersonModel?> Handle(
        ReadEntraLinkedPersonQuery request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var personEntity = await _repository.GetEntraLinkedPersonAsync(
            request.EntraObjectId, 
            cancellationToken);

        if (personEntity == null)
        {
            _logger.LogWarning(
                "Entra user not found or not linked to AdventureWorks: EntraObjectId={EntraObjectId}",
                request.EntraObjectId);
            return null;
        }

        return _mapper.Map<EntraLinkedPersonModel>(personEntity);
    }
}
