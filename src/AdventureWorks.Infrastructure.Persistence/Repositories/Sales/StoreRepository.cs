using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class StoreRepository(AdventureWorksDbContext dbContext) 
    : EfRepository<StoreEntity>(dbContext), IStoreRepository
{

    /// <summary>
    /// Retrieve a store by id along with its related entities
    /// </summary>
    /// <param name="storeId">the unique store identifier</param>
    /// <returns></returns>
    public async Task<StoreEntity> GetStoreByIdAsync(int storeId)
    {
        return await DbContext.Stores
            .AsNoTracking()
            .Include(x => x.StoreBusinessEntity)
                .ThenInclude(y => y.BusinessEntityAddresses)
                .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
                .ThenInclude(y => y.BusinessEntityAddresses)
                .ThenInclude(z => z.Address)
                .ThenInclude(ab => ab.StateProvince)
                .ThenInclude(abc => abc.CountryRegion)
            .Include(x => x.PrimarySalesPerson)
                .ThenInclude(y => y.Employee)
                .ThenInclude(z => z.PersonBusinessEntity)
                .ThenInclude(e => e.EmailAddresses)
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
            .AsNoTracking()
            .Include(x => x.StoreBusinessEntity)
                .ThenInclude(y => y.BusinessEntityAddresses)
                .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
                .ThenInclude(y => y.BusinessEntityAddresses)
                .ThenInclude(z => z.Address)
                .ThenInclude(ab => ab.StateProvince)
                .ThenInclude(abc => abc.CountryRegion)
            .Include(x => x.PrimarySalesPerson)
                .ThenInclude(y => y.Employee)
                .ThenInclude(z => z.PersonBusinessEntity)
                .ThenInclude(e => e.EmailAddresses)
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

    /// <summary>
    /// Retrieves a paged list of stores that is filtered using the <paramref name="storeSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="storeSearchModel"></param>
    /// <returns></returns>
    public async Task<(IReadOnlyList<StoreEntity>, int)> SearchStoresAsync(
        StoreParameter parameters,
        StoreSearchModel storeSearchModel)
    {
        var storeQuery = DbContext.Stores
            .AsNoTracking()
            .Include(x => x.StoreBusinessEntity)
                .ThenInclude(y => y.BusinessEntityAddresses)
                .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
                .ThenInclude(y => y.BusinessEntityAddresses)
                .ThenInclude(z => z.Address)
                .ThenInclude(ab => ab.StateProvince)
                .ThenInclude(abc => abc.CountryRegion)
            .Include(x => x.PrimarySalesPerson)
                .ThenInclude(y => y.Employee)
                .ThenInclude(z => z.PersonBusinessEntity)
                .ThenInclude(e => e.EmailAddresses)
            .AsQueryable();

        if (storeSearchModel != null)
        {
            if (storeSearchModel.Id != null)
            {
                storeQuery = storeQuery.Where( y => y.BusinessEntityId == storeSearchModel.Id );
            }

            if (!string.IsNullOrWhiteSpace(storeSearchModel.Name))
            {
                storeQuery = storeQuery.Where(y => y.Name.ToLower().Contains(storeSearchModel.Name.Trim().ToLower()));
            }
        }

        storeQuery = parameters.OrderBy switch
        {
            SortedResultConstants.BusinessEntityId => parameters.SortOrder == SortedResultConstants.Ascending
                ? storeQuery.OrderBy(x => x.BusinessEntityId)
                : storeQuery.OrderByDescending(x => x.BusinessEntityId),
            SortedResultConstants.Name => parameters.SortOrder == SortedResultConstants.Ascending
                ? storeQuery.OrderBy(x => x.Name)
                : storeQuery.OrderByDescending(x => x.Name),
            _ => storeQuery
        };

        var totalCount = await storeQuery.CountAsync();

        storeQuery = storeQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await storeQuery.ToListAsync();

        return (results.AsReadOnly(), totalCount);
    }
}

