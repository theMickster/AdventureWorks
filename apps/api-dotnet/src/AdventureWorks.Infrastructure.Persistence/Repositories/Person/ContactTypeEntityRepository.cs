using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

/// <summary>
/// Repository for ContactType entity read operations.
/// </summary>
[ServiceLifetimeScoped]
public sealed class ContactTypeEntityRepository(AdventureWorksDbContext dbContext)
    : ReadOnlyEfRepository<ContactTypeEntity>(dbContext), IContactTypeEntityRepository
{
    /// <summary>
    /// Returns true if a contact type with the given id exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.ContactTypes.AnyAsync(x => x.ContactTypeId == id, cancellationToken);
    }
}
