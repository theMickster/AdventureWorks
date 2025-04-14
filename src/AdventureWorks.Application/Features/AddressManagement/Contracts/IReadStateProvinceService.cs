using AdventureWorks.Models.Features.AddressManagement;

namespace AdventureWorks.Application.Features.AddressManagement.Contracts;

public interface IReadStateProvinceService
{

    /// <summary>
    /// Retrieve a state using its identifier.
    /// </summary>
    /// <returns>A <see cref="StateProvinceModel"/> </returns>
    Task<StateProvinceModel?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve the list of states. 
    /// </summary>
    /// <returns></returns>
    Task<List<StateProvinceModel>> GetListAsync();

}