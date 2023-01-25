using AdventureWorks.Domain.Models;

namespace AdventureWorks.Application.Interfaces.Services.CountryRegion;

public interface IReadCountryRegionService
{
    /// <summary>
    /// Retrieve the list of country regions. 
    /// </summary>
    /// <returns></returns>
    Task<List<CountryRegionModel>> GetListAsync();
}