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
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    public async Task<List<BusinessEntityContactEntity>> GetContactsByIdAsync(int businessEntityId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityContacts
            .Include(x => x.ContactType)
            .Include(x => x.Person)
            .ThenInclude(x => x.PersonType)
            .Where(x => x.BusinessEntityId == businessEntityId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the list of business contacts for a list of store (business entity) ids
    /// </summary>
    /// <param name="businessEntityIds">the list of business entity (store) identifiers</param>
    /// <returns></returns>
    public async Task<List<BusinessEntityContactEntity>> GetContactsByStoreIdsAsync(List<int> businessEntityIds, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityContacts
            .Include(x => x.ContactType)
            .Include(x => x.Person)
            .ThenInclude(x => x.PersonType)
            .Where(x => businessEntityIds.Contains(x.BusinessEntityId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns true if a contact with the given composite key exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityContacts.AnyAsync(
            x => x.BusinessEntityId == storeId && x.PersonId == personId && x.ContactTypeId == contactTypeId,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves a tracked contact by its composite key. Returns null when not found.
    /// </summary>
    public async Task<BusinessEntityContactEntity?> GetByCompositeKeyAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityContacts.FirstOrDefaultAsync(
            x => x.BusinessEntityId == storeId && x.PersonId == personId && x.ContactTypeId == contactTypeId,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves a contact (with ContactType and Person details) by its composite key. Read-only.
    /// </summary>
    public async Task<BusinessEntityContactEntity?> GetWithDetailsByCompositeKeyAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityContacts
            .AsNoTracking()
            .Include(x => x.ContactType)
            .Include(x => x.Person)
                .ThenInclude(p => p.PersonType)
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == storeId && x.PersonId == personId && x.ContactTypeId == contactTypeId,
                cancellationToken);
    }

    /// <summary>
    /// Replaces an existing contact's contact type by deleting the existing row and inserting a new one,
    /// inside a single transaction. Required because the composite primary key includes ContactTypeId.
    /// </summary>
    public async Task<BusinessEntityContactEntity> ReplaceContactTypeAsync(
        BusinessEntityContactEntity existing,
        int newContactTypeId,
        DateTime modifiedDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(existing);

        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            DbContext.BusinessEntityContacts.Remove(existing);
            await DbContext.SaveChangesAsync(cancellationToken);

            var replacement = new BusinessEntityContactEntity
            {
                BusinessEntityId = existing.BusinessEntityId,
                PersonId = existing.PersonId,
                ContactTypeId = newContactTypeId,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate
            };

            DbContext.BusinessEntityContacts.Add(replacement);
            await DbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return replacement;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
