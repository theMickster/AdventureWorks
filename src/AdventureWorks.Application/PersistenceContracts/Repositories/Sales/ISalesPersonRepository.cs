using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

public interface ISalesPersonRepository : IAsyncRepository<SalesPersonEntity>
{
    /// <summary>
    /// Retrieve a sales person by id along with their related entities
    /// </summary>
    /// <param name="salesPersonId">the unique sales person identifier</param>
    /// <returns></returns>
    Task<SalesPersonEntity?> GetSalesPersonByIdAsync(int salesPersonId);
}
