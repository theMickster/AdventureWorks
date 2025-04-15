using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadContactTypeListQueryHandler(
    IMapper mapper,
    IContactTypeRepository contactTypeRepository)
    : IRequestHandler<ReadContactTypeListQuery, List<ContactTypeModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IContactTypeRepository _contactTypeRepository = contactTypeRepository ?? throw new ArgumentNullException(nameof(contactTypeRepository));

    public async Task<List<ContactTypeModel>> Handle(ReadContactTypeListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _contactTypeRepository.ListAllAsync();

        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<ContactTypeModel>>(entities);
    }
}
