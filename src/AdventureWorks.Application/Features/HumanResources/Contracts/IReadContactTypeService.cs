using AdventureWorks.Models.Features.HumanResources;

namespace AdventureWorks.Application.Features.HumanResources.Contracts;

public interface IReadContactTypeService
{

    /// <summary>
    /// Retrieve a contact type using its identifier.
    /// </summary>
    /// <returns>A <see cref="ContactTypeModel"/> </returns>
    Task<ContactTypeModel?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve the list of contact types. 
    /// </summary>
    /// <returns></returns>
    Task<List<ContactTypeModel>> GetListAsync();

}
