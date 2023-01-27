using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.StateProvince;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Application.Services.StateProvince;

[ServiceLifetimeScoped]
public sealed class ReadStateProvinceService : IReadStateProvinceService
{
    private readonly IMapper _mapper;
    private readonly IStateProvinceRepository _stateProvinceRepository;

    public ReadStateProvinceService(
        IMapper mapper,
        IStateProvinceRepository stateProvinceRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _stateProvinceRepository = stateProvinceRepository ?? throw new ArgumentNullException(nameof(stateProvinceRepository));
    }

    /// <summary>
    /// Retrieve a state using its identifier.
    /// </summary>
    /// <returns>A <see cref="StateProvinceModel"/> </returns>
    public async Task<StateProvinceModel?> GetByIdAsync(int id)
    {
        var entity = await _stateProvinceRepository.GetByIdAsync(id).ConfigureAwait(false);

        return _mapper.Map<StateProvinceModel>(entity);
    }

    /// <summary>
    /// Retrieve the list of states. 
    /// </summary>
    /// <returns></returns>
    public async Task<List<StateProvinceModel>> GetListAsync()
    {
        var entities = await _stateProvinceRepository.ListAllAsync().ConfigureAwait(false);

        if (entities == null || !entities.Any())
        {
            return new List<StateProvinceModel>();
        }

        return _mapper.Map<List<StateProvinceEntity>,List<StateProvinceModel> >(entities.ToList());
    }
}