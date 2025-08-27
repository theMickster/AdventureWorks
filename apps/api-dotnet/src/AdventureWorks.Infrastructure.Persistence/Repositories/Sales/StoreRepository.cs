using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Common.Helpers;
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
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<StoreEntity?> GetStoreByIdAsync(int storeId, bool includeAddresses = true, CancellationToken cancellationToken = default)
    {
        return await BuildBaseStoreQuery(includeAddresses)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == storeId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated list of stores and the total count of stores in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<(IReadOnlyList<StoreEntity>, int)> GetStoresAsync(StoreParameter parameters, bool includeAddresses = true, CancellationToken cancellationToken = default)
    {
        var totalCount = await DbContext.Stores.CountAsync(cancellationToken);

        var storeQuery = BuildBaseStoreQuery(includeAddresses);

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

        var results = await storeQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a paged list of stores that is filtered using the <paramref name="storeSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="storeSearchModel"></param>
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<(IReadOnlyList<StoreEntity>, int)> SearchStoresAsync(
        StoreParameter parameters,
        StoreSearchModel storeSearchModel,
        bool includeAddresses = true,
        CancellationToken cancellationToken = default)
    {
        var storeQuery = BuildBaseStoreQuery(includeAddresses);

        if (storeSearchModel != null)
        {
            if (storeSearchModel.Id != null)
            {
                storeQuery = storeQuery.Where( y => y.BusinessEntityId == storeSearchModel.Id );
            }

            if (!string.IsNullOrWhiteSpace(storeSearchModel.Name))
            {
                storeQuery = storeQuery.Where(y => EF.Functions.Like(y.Name, $"%{LikePatternHelper.EscapeLikePattern(storeSearchModel.Name.Trim())}%"));
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

        var totalCount = await storeQuery.CountAsync(cancellationToken);

        storeQuery = storeQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await storeQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Builds the base store query with SalesPerson includes and optionally address includes.
    /// </summary>
    /// <param name="includeAddresses">when false, address navigation properties are not loaded</param>
    private IQueryable<StoreEntity> BuildBaseStoreQuery(bool includeAddresses = true)
    {
        var query = DbContext.Stores
            .AsNoTracking()
            .Include(x => x.PrimarySalesPerson)
                .ThenInclude(y => y.Employee)
                .ThenInclude(z => z.PersonBusinessEntity)
                .ThenInclude(e => e.EmailAddresses)
            .AsQueryable();

        if (includeAddresses)
        {
            query = query
                .Include(x => x.StoreBusinessEntity)
                    .ThenInclude(y => y.BusinessEntityAddresses)
                    .ThenInclude(y => y.AddressType)
                .Include(x => x.StoreBusinessEntity)
                    .ThenInclude(y => y.BusinessEntityAddresses)
                    .ThenInclude(z => z.Address)
                    .ThenInclude(ab => ab.StateProvince)
                    .ThenInclude(abc => abc.CountryRegion);
        }

        return query;
    }

    /// <summary>
    /// Returns true if a store with the given id exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Stores.AnyAsync(x => x.BusinessEntityId == id, cancellationToken);
    }
}

