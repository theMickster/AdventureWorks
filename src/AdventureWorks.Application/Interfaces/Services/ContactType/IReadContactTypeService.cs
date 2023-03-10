using AdventureWorks.Domain.Models.Person;

namespace AdventureWorks.Application.Interfaces.Services.ContactType;

public interface IReadContactTypeService
{

    /// <summary>
    /// Retrieve an contact type using its identifier.
    /// </summary>
    /// <returns>A <see cref="ContactTypeModel"/> </returns>
    Task<ContactTypeModel?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve the list of contact types. 
    /// </summary>
    /// <returns></returns>
    Task<List<ContactTypeModel>> GetListAsync();

}
