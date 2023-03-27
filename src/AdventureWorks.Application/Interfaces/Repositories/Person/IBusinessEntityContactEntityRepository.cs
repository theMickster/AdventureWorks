using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.Interfaces.Repositories.Person;

public interface IBusinessEntityContactEntityRepository : IAsyncRepository<BusinessEntityContactEntity>
{
    /// <summary>
    /// Retrieve the list of business contacts for a given store (business entity) id
    /// </summary>
    /// <param name="businessEntityId">the unique business entity (store) identifier</param>
    /// <returns></returns>
    Task<List<BusinessEntityContactEntity>> GetContactsByIdAsync(int businessEntityId);

    /// <summary>
    /// Retrieves the list of business contacts for a list of store (business entity) ids
    /// </summary>
    /// <param name="businessEntityIds">the list of business entity (store) identifiers</param>
    /// <returns></returns>
    Task<List<BusinessEntityContactEntity>> GetContactsByStoreIdsAsync(List<int> businessEntityIds);
}
