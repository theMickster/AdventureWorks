using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Services.PersonType;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Models.Person;
using AutoMapper;

namespace AdventureWorks.Application.Services.PersonType;

[ServiceLifetimeScoped]
public sealed class ReadPersonTypeService : IReadPersonTypeService
{
    private readonly IMapper _mapper;
    private readonly IPersonTypeRepository _personTypeRepository;

    public ReadPersonTypeService(
        IMapper mapper,
        IPersonTypeRepository personTypeRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _personTypeRepository = personTypeRepository ?? throw new ArgumentNullException(nameof(personTypeRepository));
    }

    /// <summary>
    /// Retrieve an person type using its identifier.
    /// </summary>
    /// <returns>A <see cref="PersonTypeModel"/> </returns>
    public async Task<PersonTypeModel?> GetByIdAsync(int id)
    {
        var entity = await _personTypeRepository.GetByIdAsync(id).ConfigureAwait(false);

        return _mapper.Map<PersonTypeModel>(entity);
    }

    /// <summary>
    /// Retrieve the list of person types. 
    /// </summary>
    /// <returns></returns>
    public async Task<List<PersonTypeModel>> GetListAsync()
    {
        var entities = await _personTypeRepository.ListAllAsync().ConfigureAwait(false);

        if (entities == null || !entities.Any())
        {
            return new List<PersonTypeModel>();
        }

        return _mapper.Map<List<PersonTypeModel>>(entities);
    }
}
