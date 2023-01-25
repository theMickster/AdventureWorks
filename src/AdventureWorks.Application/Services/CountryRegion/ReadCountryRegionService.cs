using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.CountryRegion;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Application.Services.CountryRegion;

[ServiceLifetimeScoped]
public sealed class ReadCountryRegionService : IReadCountryRegionService
{
    private readonly ICountryRegionRepository _countryRegionRepository;
    private readonly IMapper _mapper;

    public ReadCountryRegionService(IMapper mapper, ICountryRegionRepository countryRegionRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _countryRegionRepository = countryRegionRepository ?? throw new ArgumentNullException(nameof(countryRegionRepository));
    }

    /// <summary>
    /// Retrieve a country region using its identifier.
    /// </summary>
    /// <returns>A <see cref="CountryRegionModel"/> </returns>
    public async Task<CountryRegionModel> GetByIdAsync(string countryCode)
    {
        var entity = await _countryRegionRepository.GetByIdAsync(countryCode).ConfigureAwait(false);

        return _mapper.Map<CountryRegionModel>(entity);
    }

    /// <summary>
    /// Retrieve the list of country regions. 
    /// </summary>
    /// <returns></returns>
    public async Task<List<CountryRegionModel>> GetListAsync()
    {
        var entities = await _countryRegionRepository.ListAllAsync().ConfigureAwait(false);

        if (entities == null || !entities.Any())
        {
            return new List<CountryRegionModel>();
        }

        return _mapper.Map<List<CountryRegionModel>>(entities);
    }
}