using AdventureWorks.Application.Features.HumanResources.Contracts;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Services.ContactType;

[ServiceLifetimeScoped]
public sealed class ReadContactTypeService : IReadContactTypeService
{
    private readonly IMapper _mapper;
    private readonly IContactTypeRepository _contactTypeRepository;

    public ReadContactTypeService(
        IMapper mapper,
        IContactTypeRepository contactTypeRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _contactTypeRepository = contactTypeRepository ?? throw new ArgumentNullException(nameof(contactTypeRepository));
    }

    /// <summary>
    /// Retrieve an contact type using its identifier.
    /// </summary>
    /// <returns>A <see cref="ContactTypeModel"/> </returns>
    public async Task<ContactTypeModel?> GetByIdAsync(int id)
    {
        var entity = await _contactTypeRepository.GetByIdAsync(id).ConfigureAwait(false);

        return _mapper.Map<ContactTypeModel>(entity);
    }

    /// <summary>
    /// Retrieve the list of contact types. 
    /// </summary>
    /// <returns></returns>
    public async Task<List<ContactTypeModel>> GetListAsync()
    {
        var entities = await _contactTypeRepository.ListAllAsync().ConfigureAwait(false);

        if (entities == null || !entities.Any())
        {
            return new List<ContactTypeModel>();
        }

        return _mapper.Map<List<ContactTypeModel>>(entities);
    }
}
