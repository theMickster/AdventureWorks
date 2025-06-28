using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface ISalesTerritoryRepository : IReadOnlyAsyncRepository<SalesTerritoryEntity>
{

}
