using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadPersonTypeListQueryHandler(
    IMapper mapper,
    IPersonTypeRepository personTypeRepository)
    : IRequestHandler<ReadPersonTypeListQuery, List<PersonTypeModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonTypeRepository _personTypeRepository = personTypeRepository ?? throw new ArgumentNullException(nameof(personTypeRepository));

    public async Task<List<PersonTypeModel>> Handle(ReadPersonTypeListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _personTypeRepository.ListAllAsync();

        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<PersonTypeModel>>(entities);
    }
}
