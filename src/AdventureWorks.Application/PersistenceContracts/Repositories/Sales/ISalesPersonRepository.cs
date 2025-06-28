using AdventureWorks.Common.Filtering;
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

    /// <summary>
    /// Retrieves a paginated list of sales persons and the total count of sales persons in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    Task<(IReadOnlyList<SalesPersonEntity>, int)> GetSalesPersonsAsync(SalesPersonParameter parameters);

    /// <summary>
    /// Retrieves a paged list of sales persons that is filtered using the <paramref name="salesPersonSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="salesPersonSearchModel"></param>
    /// <returns></returns>
    Task<(IReadOnlyList<SalesPersonEntity>, int)> SearchSalesPersonsAsync(SalesPersonParameter parameters, SalesPersonSearchModel salesPersonSearchModel);
}
