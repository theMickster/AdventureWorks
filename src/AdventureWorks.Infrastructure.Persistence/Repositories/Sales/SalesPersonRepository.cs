using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class SalesPersonRepository(AdventureWorksDbContext dbContext)
    : EfRepository<SalesPersonEntity>(dbContext), ISalesPersonRepository
{
    /// <summary>
    /// Retrieve a sales person by id along with their related entities
    /// </summary>
    /// <param name="salesPersonId">the unique sales person identifier</param>
    /// <returns></returns>
    public async Task<SalesPersonEntity?> GetSalesPersonByIdAsync(int salesPersonId)
    {
        return await DbContext.SalesPersons
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(y => y.PersonBusinessEntity)
                .ThenInclude(z => z.EmailAddresses)
            .Include(x => x.SalesTerritory)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == salesPersonId);
    }
}
