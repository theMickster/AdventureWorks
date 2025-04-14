using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public class ReadPersonTypeQueryHandler(
    IMapper mapper,
    IPersonTypeRepository personTypeRepository) : IRequestHandler<ReadPersonTypeQuery, PersonTypeModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonTypeRepository _personTypeRepository = personTypeRepository ?? throw new ArgumentNullException(nameof(personTypeRepository));

    /// <summary>
    /// Retrieve a person type using its unique identifier.
    /// </summary>
    /// <returns>A <see cref="PersonTypeModel"/> </returns>
    public async Task<PersonTypeModel> Handle(ReadPersonTypeQuery request, CancellationToken cancellationToken)
    {        
        return _mapper.Map<PersonTypeModel>(await _personTypeRepository.GetByIdAsync(request.Id));
    }
}
