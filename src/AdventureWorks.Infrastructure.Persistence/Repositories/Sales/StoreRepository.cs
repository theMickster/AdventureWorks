using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class StoreRepository : EfRepository<StoreEntity>, IStoreRepository
{
    public StoreRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique store identifier</param>
    /// <returns></returns>
    public async Task<StoreEntity> GetStoreByIdAsync(int storeId)
    {
        return await DbContext.Stores
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(z => z.Address)
            .ThenInclude(ab => ab.StateProvince)
            .ThenInclude(abc => abc.CountryRegion)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == storeId);
    }

    /// <summary>
    /// Retrieves a paginated list of stores and the total count of stores in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    public async Task<(IReadOnlyList<StoreEntity>, int)> GetStoresAsync(StoreParameter parameters)
    {
        var totalCount = await DbContext.Stores.CountAsync();

        var storeQuery = DbContext.Stores
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(z => z.Address)
            .ThenInclude(ab => ab.StateProvince)
            .ThenInclude(abc => abc.CountryRegion)
            .AsQueryable();

        switch (parameters.OrderBy)
        {
            case SortedResultConstants.BusinessEntityId:
                storeQuery = parameters.SortOrder == SortedResultConstants.Ascending ? 
                    storeQuery.OrderBy(x => x.BusinessEntityId) : 
                    storeQuery.OrderByDescending(x => x.BusinessEntityId);
                break;
            case SortedResultConstants.Name:
                storeQuery = parameters.SortOrder == SortedResultConstants.Ascending ?
                    storeQuery.OrderBy(x => x.Name) :
                    storeQuery.OrderByDescending(x => x.Name);
                break;
        }

        storeQuery = storeQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await storeQuery.ToListAsync();

        return (results.AsReadOnly(), totalCount);
    }
}
