using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class PersonEmailRepository(AdventureWorksDbContext dbContext)
    : EfRepository<EmailAddressEntity>(dbContext), IPersonEmailRepository
{
    /// <summary>
    /// Retrieves all email addresses for the specified person. Read-only.
    /// </summary>
    public async Task<List<EmailAddressEntity>> GetEmailsByPersonIdAsync(int businessEntityId, CancellationToken cancellationToken = default)
    {
        return await DbContext.EmailAddresses
            .AsNoTracking()
            .Where(x => x.BusinessEntityId == businessEntityId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a tracked email address by its composite key. Returns null when not found.
    /// Tracking is required for update operations.
    /// </summary>
    public async Task<EmailAddressEntity?> GetEmailByCompositeKeyAsync(int businessEntityId, int emailAddressId, CancellationToken cancellationToken = default)
    {
        return await DbContext.EmailAddresses
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == businessEntityId && x.EmailAddressId == emailAddressId,
                cancellationToken);
    }

    /// <summary>
    /// Returns true if a Person record with the given BusinessEntityId exists.
    /// </summary>
    public async Task<bool> PersonExistsAsync(int businessEntityId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Persons.AnyAsync(x => x.BusinessEntityId == businessEntityId, cancellationToken);
    }

    /// <summary>
    /// Returns true if an email address already exists for the specified person.
    /// Direct equality lets the DB collation handle case sensitivity and allows index use.
    /// </summary>
    public async Task<bool> EmailExistsForPersonAsync(int businessEntityId, string emailAddress, CancellationToken cancellationToken = default)
    {
        return await DbContext.EmailAddresses
            .AsNoTracking()
            .AnyAsync(
                x => x.BusinessEntityId == businessEntityId && x.EmailAddressName == emailAddress,
                cancellationToken);
    }

    /// <summary>
    /// Hard-deletes the email address identified by the composite key.
    /// </summary>
    public async Task DeleteEmailAsync(int businessEntityId, int emailAddressId, CancellationToken cancellationToken = default)
    {
        var entity = await DbContext.EmailAddresses
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == businessEntityId && x.EmailAddressId == emailAddressId,
                cancellationToken);

        if (entity is not null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }
}
