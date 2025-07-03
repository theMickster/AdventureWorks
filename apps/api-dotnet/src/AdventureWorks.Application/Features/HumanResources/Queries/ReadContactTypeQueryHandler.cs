using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadContactTypeQueryHandler(
    IMapper mapper,
    IContactTypeRepository contactTypeRepository) : IRequestHandler<ReadContactTypeQuery, ContactTypeModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IContactTypeRepository _contactTypeRepository = contactTypeRepository ?? throw new ArgumentNullException(nameof(contactTypeRepository));

    public async Task<ContactTypeModel> Handle(ReadContactTypeQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<ContactTypeModel>(await _contactTypeRepository.GetByIdAsync(request.Id));
    }
}
