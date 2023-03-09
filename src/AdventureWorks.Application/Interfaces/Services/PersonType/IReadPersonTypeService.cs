using AdventureWorks.Domain.Models.Person;

namespace AdventureWorks.Application.Interfaces.Services.PersonType;

public interface IReadPersonTypeService
{

    /// <summary>
    /// Retrieve an person type using its identifier.
    /// </summary>
    /// <returns>A <see cref="PersonTypeModel"/> </returns>
    Task<PersonTypeModel?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve the list of person types. 
    /// </summary>
    /// <returns></returns>
    Task<List<PersonTypeModel>> GetListAsync();

}
