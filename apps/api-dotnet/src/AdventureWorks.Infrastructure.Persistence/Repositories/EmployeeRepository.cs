using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Common.Helpers;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for Employee entity with cascade operations for Person creation.
/// </summary>
[ServiceLifetimeScoped]
public sealed class EmployeeRepository(AdventureWorksDbContext dbContext)
    : EfRepository<EmployeeEntity>(dbContext), IEmployeeRepository
{
    /// <summary>
    /// Creates a new employee with full entity graph using EF Core navigation properties.
    /// Leverages EF Core's change tracking for cascade inserts.
    /// All operations are performed within a single transaction.
    /// </summary>
    public async Task<int> CreateEmployeeWithPersonAsync(
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
    /// Retrieves an employee by their BusinessEntityId with related Person data.
    /// </summary>
    /// <param name="businessEntityId">The employee's business entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee entity with navigation properties, or null if not found</returns>
    public async Task<EmployeeEntity?> GetEmployeeByIdAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Employees
            .Include(e => e.PersonBusinessEntity)
                .ThenInclude(p => p.EmailAddresses)
            .Where(e => e.BusinessEntityId == businessEntityId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated list of employees and the total count of employees in the database.
    /// </summary>
    public async Task<(IReadOnlyList<EmployeeEntity>, int)> GetEmployeesAsync(
        EmployeeParameter parameters,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await DbContext.Employees.CountAsync(cancellationToken);

        var employeeQuery = DbContext.Employees
            .AsNoTracking()
            .Include(e => e.PersonBusinessEntity)
                .ThenInclude(p => p.EmailAddresses)
            .AsQueryable();

        employeeQuery = parameters.OrderBy switch
        {
            SortedResultConstants.BusinessEntityId => parameters.SortOrder == SortedResultConstants.Ascending
                ? employeeQuery.OrderBy(x => x.BusinessEntityId)
                : employeeQuery.OrderByDescending(x => x.BusinessEntityId),
            SortedResultConstants.FirstName => parameters.SortOrder == SortedResultConstants.Ascending
                ? employeeQuery.OrderBy(x => x.PersonBusinessEntity.FirstName)
                : employeeQuery.OrderByDescending(x => x.PersonBusinessEntity.FirstName),
            SortedResultConstants.LastName => parameters.SortOrder == SortedResultConstants.Ascending
                ? employeeQuery.OrderBy(x => x.PersonBusinessEntity.LastName)
                : employeeQuery.OrderByDescending(x => x.PersonBusinessEntity.LastName),
            _ => employeeQuery.OrderBy(x => x.BusinessEntityId)
        };

        employeeQuery = employeeQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await employeeQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a paged list of employees filtered using the search criteria.
    /// </summary>
    public async Task<(IReadOnlyList<EmployeeEntity>, int)> SearchEmployeesAsync(
        EmployeeParameter parameters,
        EmployeeSearchModel employeeSearchModel,
        CancellationToken cancellationToken = default)
    {
        var employeeQuery = DbContext.Employees
            .AsNoTracking()
            .Include(e => e.PersonBusinessEntity)
                .ThenInclude(p => p.EmailAddresses)
            .AsQueryable();

        if (employeeSearchModel != null)
        {
            if (employeeSearchModel.Id != null)
            {
                employeeQuery = employeeQuery.Where(e => e.BusinessEntityId == employeeSearchModel.Id);
            }

            if (!string.IsNullOrWhiteSpace(employeeSearchModel.FirstName))
            {
                employeeQuery = employeeQuery.Where(e =>
                    EF.Functions.Like(e.PersonBusinessEntity.FirstName, $"%{LikePatternHelper.EscapeLikePattern(employeeSearchModel.FirstName.Trim())}%"));
            }

            if (!string.IsNullOrWhiteSpace(employeeSearchModel.LastName))
            {
                employeeQuery = employeeQuery.Where(e =>
                    EF.Functions.Like(e.PersonBusinessEntity.LastName, $"%{LikePatternHelper.EscapeLikePattern(employeeSearchModel.LastName.Trim())}%"));
            }

            if (!string.IsNullOrWhiteSpace(employeeSearchModel.JobTitle))
            {
                employeeQuery = employeeQuery.Where(e =>
                    EF.Functions.Like(e.JobTitle, $"%{LikePatternHelper.EscapeLikePattern(employeeSearchModel.JobTitle.Trim())}%"));
            }

            if (!string.IsNullOrWhiteSpace(employeeSearchModel.EmailAddress))
            {
                employeeQuery = employeeQuery.Where(e =>
                    e.PersonBusinessEntity.EmailAddresses.Any(email =>
                        EF.Functions.Like(email.EmailAddressName, $"%{LikePatternHelper.EscapeLikePattern(employeeSearchModel.EmailAddress.Trim())}%")));
            }

            if (!string.IsNullOrWhiteSpace(employeeSearchModel.NationalIdNumber))
            {
                employeeQuery = employeeQuery.Where(e =>
                    EF.Functions.Like(e.NationalIdnumber, $"%{LikePatternHelper.EscapeLikePattern(employeeSearchModel.NationalIdNumber.Trim())}%"));
            }

            if (!string.IsNullOrWhiteSpace(employeeSearchModel.LoginId))
            {
                employeeQuery = employeeQuery.Where(e =>
                    EF.Functions.Like(e.LoginId, $"%{LikePatternHelper.EscapeLikePattern(employeeSearchModel.LoginId.Trim())}%"));
            }
        }

        employeeQuery = parameters.OrderBy switch
        {
            SortedResultConstants.BusinessEntityId => parameters.SortOrder == SortedResultConstants.Ascending
                ? employeeQuery.OrderBy(x => x.BusinessEntityId)
                : employeeQuery.OrderByDescending(x => x.BusinessEntityId),
            SortedResultConstants.FirstName => parameters.SortOrder == SortedResultConstants.Ascending
                ? employeeQuery.OrderBy(x => x.PersonBusinessEntity.FirstName)
                : employeeQuery.OrderByDescending(x => x.PersonBusinessEntity.FirstName),
            SortedResultConstants.LastName => parameters.SortOrder == SortedResultConstants.Ascending
                ? employeeQuery.OrderBy(x => x.PersonBusinessEntity.LastName)
                : employeeQuery.OrderByDescending(x => x.PersonBusinessEntity.LastName),
            _ => employeeQuery
        };

        var totalCount = await employeeQuery.CountAsync(cancellationToken);

        employeeQuery = employeeQuery.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await employeeQuery.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves all addresses for a specific employee with related Address and AddressType data.
    /// </summary>
    public async Task<IReadOnlyList<BusinessEntityAddressEntity>> GetEmployeeAddressesAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        var addresses = await DbContext.BusinessEntityAddresses
            .AsNoTracking()
            .Include(bea => bea.Address)
                .ThenInclude(a => a.StateProvince)
                    .ThenInclude(sp => sp.CountryRegion)
            .Include(bea => bea.AddressType)
            .Where(bea => bea.BusinessEntityId == businessEntityId)
            .ToListAsync(cancellationToken);

        return addresses.AsReadOnly();
    }

    /// <summary>
    /// Retrieves a specific address for an employee with related Address and AddressType data.
    /// </summary>
    public async Task<BusinessEntityAddressEntity?> GetEmployeeAddressByIdAsync(
        int businessEntityId,
        int addressId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityAddresses
            .AsNoTracking()
            .Include(bea => bea.Address)
                .ThenInclude(a => a.StateProvince)
                    .ThenInclude(sp => sp.CountryRegion)
            .Include(bea => bea.AddressType)
            .Where(bea => bea.BusinessEntityId == businessEntityId && bea.AddressId == addressId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves an employee by their BusinessEntityId with EmployeeDepartmentHistory collection.
    /// Includes related Department and Shift entities for each history record.
    /// Used by lifecycle commands (Terminate, Rehire) that need to manage department assignments.
    /// </summary>
    public async Task<EmployeeEntity?> GetEmployeeByIdWithDepartmentHistoryAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Employees
            .Include(e => e.EmployeeDepartmentHistory)
                .ThenInclude(dh => dh.Department)
            .Include(e => e.EmployeeDepartmentHistory)
                .ThenInclude(dh => dh.Shift)
            .Where(e => e.BusinessEntityId == businessEntityId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all department history records for an employee, ordered by StartDate descending.
    /// Includes related Department and Shift entities.
    /// </summary>
    public async Task<IReadOnlyList<EmployeeDepartmentHistory>> GetEmployeeDepartmentHistoryAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        var records = await DbContext.EmployeeDepartmentHistories
            .AsNoTracking()
            .Include(dh => dh.Department)
            .Include(dh => dh.Shift)
            .Where(dh => dh.BusinessEntityId == businessEntityId)
            .OrderByDescending(dh => dh.StartDate)
            .ToListAsync(cancellationToken);

        return records.AsReadOnly();
    }

    /// <summary>
    /// Retrieves an employee by their BusinessEntityId with full lifecycle data.
    /// Includes Person, EmployeeDepartmentHistory (with Department and Shift), and EmployeePayHistory.
    /// Used by lifecycle status query to aggregate comprehensive employee information.
    /// Uses AsNoTracking for read-only queries to improve performance.
    /// </summary>
    public async Task<EmployeeEntity?> GetEmployeeByIdWithLifecycleDataAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Employees
            .AsNoTracking()
            .Include(e => e.PersonBusinessEntity)
            .Include(e => e.EmployeeDepartmentHistory)
                .ThenInclude(dh => dh.Department)
            .Include(e => e.EmployeeDepartmentHistory)
                .ThenInclude(dh => dh.Shift)
            .Include(e => e.EmployeePayHistory)
            .Where(e => e.BusinessEntityId == businessEntityId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all pay history records for an employee, ordered by RateChangeDate descending.
    /// </summary>
    public async Task<IReadOnlyList<EmployeePayHistory>> GetEmployeePayHistoryAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        var records = await DbContext.EmployeePayHistories
            .AsNoTracking()
            .Where(x => x.BusinessEntityId == businessEntityId)
            .OrderByDescending(x => x.RateChangeDate)
            .ToListAsync(cancellationToken);

        return records.AsReadOnly();
    }

    /// <summary>
    /// Inserts a new pay history record and saves changes.
    /// </summary>
    public async Task RecordPayChangeAsync(
        EmployeePayHistory record,
        CancellationToken cancellationToken = default)
    {
        await DbContext.EmployeePayHistories.AddAsync(record, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Transfers an employee to a new department and/or shift within a single transaction.
    /// Closes the active department assignment (sets EndDate) and inserts a new open record.
    /// Re-fetches the active record inside the transaction to prevent stale-data races between
    /// the handler's validation read and the actual write.
    /// </summary>
    public async Task TransferEmployeeDepartmentAsync(
        int businessEntityId,
        short newDepartmentId,
        byte newShiftId,
        DateTime transferDate,
        DateTime modifiedDate,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingConflict = await DbContext.EmployeeDepartmentHistories
                .AnyAsync(dh => dh.BusinessEntityId == businessEntityId
                             && dh.DepartmentId == newDepartmentId
                             && dh.ShiftId == newShiftId
                             && dh.StartDate == transferDate, cancellationToken);

            if (existingConflict)
            {
                throw new ConflictException(
                    $"A department history record already exists for employee {businessEntityId} " +
                    $"in department {newDepartmentId} on {transferDate:yyyy-MM-dd}. " +
                    "Cannot transfer more than once per day to the same department and shift.");
            }

            var activeRecord = await DbContext.EmployeeDepartmentHistories
                .FirstOrDefaultAsync(
                    dh => dh.BusinessEntityId == businessEntityId && dh.EndDate == null,
                    cancellationToken);

            if (activeRecord is null)
            {
                throw new ConflictException($"Employee {businessEntityId} has no active department assignment to close.");
            }

            activeRecord.EndDate = transferDate;
            activeRecord.ModifiedDate = modifiedDate;

            var newRecord = new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = newDepartmentId,
                ShiftId = newShiftId,
                StartDate = transferDate,
                EndDate = null,
                ModifiedDate = modifiedDate
            };

            DbContext.EmployeeDepartmentHistories.Add(newRecord);
            await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
