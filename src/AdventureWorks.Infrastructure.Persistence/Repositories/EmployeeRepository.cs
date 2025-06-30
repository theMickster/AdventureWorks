using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
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
    public async Task<EmployeeEntity?> GetEmployeeByIdAsync(
        int businessEntityId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Employees
            .AsNoTracking()
            .Include(e => e.PersonBusinessEntity)
            .Where(e => e.BusinessEntityId == businessEntityId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
