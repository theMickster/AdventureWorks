using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
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

    /// <summary>
    /// Retrieves a paginated list of sales persons and the total count of sales persons in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    public async Task<(IReadOnlyList<SalesPersonEntity>, int)> GetSalesPersonsAsync(SalesPersonParameter parameters)
    {
        var totalCount = await DbContext.SalesPersons.CountAsync();

        var salesPersonQuery = DbContext.SalesPersons
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(y => y.PersonBusinessEntity)
                .ThenInclude(z => z.EmailAddresses)
            .Include(x => x.SalesTerritory)
            .AsQueryable();

        switch (parameters.OrderBy)
        {
            case SortedResultConstants.BusinessEntityId:
                salesPersonQuery = parameters.SortOrder == SortedResultConstants.Ascending
                    ? salesPersonQuery.OrderBy(x => x.BusinessEntityId)
                    : salesPersonQuery.OrderByDescending(x => x.BusinessEntityId);
                break;
            case SortedResultConstants.FirstName:
                salesPersonQuery = parameters.SortOrder == SortedResultConstants.Ascending
                    ? salesPersonQuery.OrderBy(x => x.Employee.PersonBusinessEntity.FirstName)
                    : salesPersonQuery.OrderByDescending(x => x.Employee.PersonBusinessEntity.FirstName);
                break;
            case SortedResultConstants.LastName:
                salesPersonQuery = parameters.SortOrder == SortedResultConstants.Ascending
                    ? salesPersonQuery.OrderBy(x => x.Employee.PersonBusinessEntity.LastName)
                    : salesPersonQuery.OrderByDescending(x => x.Employee.PersonBusinessEntity.LastName);
                break;
        }

        salesPersonQuery = salesPersonQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await salesPersonQuery.ToListAsync();

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a paged list of sales persons that is filtered using the <paramref name="salesPersonSearchModel"/> input parameter.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="salesPersonSearchModel"></param>
    /// <returns></returns>
    public async Task<(IReadOnlyList<SalesPersonEntity>, int)> SearchSalesPersonsAsync(
        SalesPersonParameter parameters,
        SalesPersonSearchModel salesPersonSearchModel)
    {
        var salesPersonQuery = DbContext.SalesPersons
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(y => y.PersonBusinessEntity)
                .ThenInclude(z => z.EmailAddresses)
            .Include(x => x.SalesTerritory)
            .AsQueryable();

        if (salesPersonSearchModel != null)
        {
            if (salesPersonSearchModel.Id != null)
            {
                salesPersonQuery = salesPersonQuery.Where(y => y.BusinessEntityId == salesPersonSearchModel.Id);
            }

            if (!string.IsNullOrWhiteSpace(salesPersonSearchModel.FirstName))
            {
                salesPersonQuery = salesPersonQuery.Where(y =>
                    y.Employee.PersonBusinessEntity.FirstName.ToLower().Contains(salesPersonSearchModel.FirstName.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(salesPersonSearchModel.LastName))
            {
                salesPersonQuery = salesPersonQuery.Where(y =>
                    y.Employee.PersonBusinessEntity.LastName.ToLower().Contains(salesPersonSearchModel.LastName.Trim().ToLower()));
            }

            if (salesPersonSearchModel.SalesTerritoryId != null)
            {
                salesPersonQuery = salesPersonQuery.Where(y => y.TerritoryId == salesPersonSearchModel.SalesTerritoryId);
            }

            if (!string.IsNullOrWhiteSpace(salesPersonSearchModel.SalesTerritoryName))
            {
                salesPersonQuery = salesPersonQuery.Where(y =>
                    y.SalesTerritory != null &&
                    y.SalesTerritory.Name.ToLower().Contains(salesPersonSearchModel.SalesTerritoryName.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(salesPersonSearchModel.SalesTerritoryGroupName))
            {
                salesPersonQuery = salesPersonQuery.Where(y =>
                    y.SalesTerritory != null &&
                    y.SalesTerritory.Group.ToLower().Contains(salesPersonSearchModel.SalesTerritoryGroupName.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(salesPersonSearchModel.EmailAddress))
            {
                salesPersonQuery = salesPersonQuery.Where(y =>
                    y.Employee.PersonBusinessEntity.EmailAddresses.Any(e =>
                        e.EmailAddressName.ToLower().Contains(salesPersonSearchModel.EmailAddress.Trim().ToLower())));
            }
        }

        salesPersonQuery = parameters.OrderBy switch
        {
            SortedResultConstants.BusinessEntityId => parameters.SortOrder == SortedResultConstants.Ascending
                ? salesPersonQuery.OrderBy(x => x.BusinessEntityId)
                : salesPersonQuery.OrderByDescending(x => x.BusinessEntityId),
            SortedResultConstants.FirstName => parameters.SortOrder == SortedResultConstants.Ascending
                ? salesPersonQuery.OrderBy(x => x.Employee.PersonBusinessEntity.FirstName)
                : salesPersonQuery.OrderByDescending(x => x.Employee.PersonBusinessEntity.FirstName),
            SortedResultConstants.LastName => parameters.SortOrder == SortedResultConstants.Ascending
                ? salesPersonQuery.OrderBy(x => x.Employee.PersonBusinessEntity.LastName)
                : salesPersonQuery.OrderByDescending(x => x.Employee.PersonBusinessEntity.LastName),
            _ => salesPersonQuery
        };

        var totalCount = await salesPersonQuery.CountAsync();

        salesPersonQuery = salesPersonQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await salesPersonQuery.ToListAsync();

        return (results.AsReadOnly(), totalCount);
    }
}
