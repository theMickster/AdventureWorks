using System.Collections.Generic;
using System.Linq;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class BusinessEntityContactEntityRepository : EfRepository<BusinessEntityContactEntity>, IBusinessEntityContactEntityRepository
{
    public BusinessEntityContactEntityRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {

    }

    /// <summary>
    /// Retrieve the list of business contacts for a given store (business entity) id
    /// </summary>
    /// <param name="businessEntityId">the unique business entity (store) identifier</param>
    /// <returns></returns>
    public async Task<List<BusinessEntityContactEntity>> GetContactsByIdAsync(int businessEntityId)
    {
        return await DbContext.BusinessEntityContacts
            .Include(x => x.ContactType)
            .Include(x => x.Person)
            .ThenInclude(x => x.PersonType)
            .Where(x => x.BusinessEntityId == businessEntityId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the list of business contacts for a list of store (business entity) ids
    /// </summary>
    /// <param name="businessEntityIds">the list of business entity (store) identifiers</param>
    /// <returns></returns>
    public async Task<List<BusinessEntityContactEntity>> GetContactsByStoreIdsAsync(List<int> businessEntityIds)
    {
        return await DbContext.BusinessEntityContacts
            .Include(x => x.ContactType)
            .Include(x => x.Person)
            .ThenInclude(x => x.PersonType)
            .Where(x => businessEntityIds.Contains(x.BusinessEntityId))
            .ToListAsync();
    }
}
