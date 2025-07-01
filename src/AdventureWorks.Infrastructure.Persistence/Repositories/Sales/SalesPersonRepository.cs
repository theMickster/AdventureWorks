using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

[ServiceLifetimeScoped]
public sealed class SalesPersonRepository(AdventureWorksDbContext dbContext)
    : EfRepository<SalesPersonEntity>(dbContext), ISalesPersonRepository
{
    /// <summary>
    /// Creates a new sales person with full entity graph using EF Core navigation properties.
    /// Leverages EF Core's change tracking for cascade inserts.
    /// All operations are performed within a single transaction.
    /// Creates: BusinessEntity → Person → Employee → SalesPerson + Phone + Email + Address.
    /// </summary>
    public async Task<int> CreateSalesPersonWithEmployeeAsync(
        SalesPersonEntity salesPersonEntity,
        EmployeeEntity employeeEntity,
        PersonEntity personEntity,
        PersonPhone personPhone,
        EmailAddressEntity emailAddress,
        AddressEntity address,
        int addressTypeId,
        DateTime modifiedDate,
        Guid rowGuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(salesPersonEntity);
        ArgumentNullException.ThrowIfNull(employeeEntity);
        ArgumentNullException.ThrowIfNull(personEntity);
        ArgumentNullException.ThrowIfNull(personPhone);
        ArgumentNullException.ThrowIfNull(emailAddress);
        ArgumentNullException.ThrowIfNull(address);

        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var businessEntity = new BusinessEntity
            {
                Rowguid = rowGuid,
                ModifiedDate = modifiedDate
            };

            DbContext.BusinessEntities.Add(businessEntity);
            await DbContext.SaveChangesAsync(cancellationToken);

            var businessEntityId = businessEntity.BusinessEntityId;

            personEntity.BusinessEntityId = businessEntityId;
            personEntity.PersonTypeId = 2;
            personEntity.NameStyle = false;
            personEntity.EmailPromotion = 0;
            personEntity.Rowguid = Guid.NewGuid();
            personEntity.ModifiedDate = modifiedDate;

            employeeEntity.BusinessEntityId = businessEntityId;
            employeeEntity.CurrentFlag = true;
            employeeEntity.VacationHours = 0;
            employeeEntity.SickLeaveHours = 0;
            employeeEntity.Rowguid = Guid.NewGuid();
            employeeEntity.ModifiedDate = modifiedDate;

            employeeEntity.PersonBusinessEntity = personEntity;

            DbContext.Employees.Add(employeeEntity);
            await DbContext.SaveChangesAsync(cancellationToken);

            salesPersonEntity.BusinessEntityId = businessEntityId;
            salesPersonEntity.SalesYtd = 0;
            salesPersonEntity.SalesLastYear = 0;
            salesPersonEntity.Rowguid = Guid.NewGuid();
            salesPersonEntity.ModifiedDate = modifiedDate;

            salesPersonEntity.Employee = employeeEntity;

            DbContext.SalesPersons.Add(salesPersonEntity);
            await DbContext.SaveChangesAsync(cancellationToken);

            personPhone.BusinessEntityId = businessEntityId;
            personPhone.ModifiedDate = modifiedDate;
            personEntity.PersonPhones ??= new List<PersonPhone>();
            personEntity.PersonPhones.Add(personPhone);

            await DbContext.SaveChangesAsync(cancellationToken);

            emailAddress.BusinessEntityId = businessEntityId;
            emailAddress.Rowguid = Guid.NewGuid();
            emailAddress.ModifiedDate = modifiedDate;

            personEntity.EmailAddresses ??= new List<EmailAddressEntity>();
            personEntity.EmailAddresses.Add(emailAddress);

            await DbContext.SaveChangesAsync(cancellationToken);

            address.Rowguid = Guid.NewGuid();
            address.ModifiedDate = modifiedDate;

            DbContext.Addresses.Add(address);
            await DbContext.SaveChangesAsync(cancellationToken);

            var businessEntityAddress = new BusinessEntityAddressEntity
            {
                BusinessEntityId = businessEntityId,
                AddressId = address.AddressId,
                AddressTypeId = addressTypeId,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate
            };

            DbContext.BusinessEntityAddresses.Add(businessEntityAddress);
            await DbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return businessEntityId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing sales person with cascade updates to related entities.
    /// Updates: Person → Employee → SalesPerson.
    /// All operations are performed within a single transaction.
    /// </summary>
    public async Task UpdateSalesPersonWithEmployeeAsync(
        SalesPersonEntity salesPersonEntity,
        EmployeeEntity employeeEntity,
        PersonEntity personEntity,
        DateTime modifiedDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(salesPersonEntity);
        ArgumentNullException.ThrowIfNull(employeeEntity);
        ArgumentNullException.ThrowIfNull(personEntity);

        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update Person entity
            personEntity.ModifiedDate = modifiedDate;
            DbContext.Persons.Update(personEntity);

            // Update Employee entity
            employeeEntity.ModifiedDate = modifiedDate;
            DbContext.Employees.Update(employeeEntity);

            // Update SalesPerson entity
            salesPersonEntity.ModifiedDate = modifiedDate;
            DbContext.SalesPersons.Update(salesPersonEntity);

            await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

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
