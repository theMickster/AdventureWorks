using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

/// <summary>
/// Repository for Person entity operations with Entra user validation.
/// </summary>
[ServiceLifetimeScoped]
public sealed class PersonRepository(AdventureWorksDbContext dbContext)
    : ReadOnlyEfRepository<PersonEntity>(dbContext), IPersonRepository
{
    /// <summary>
    /// Retrieves a Person entity linked to an Entra user with validation.
    /// Performs checks: BusinessEntity.Rowguid matches, IsEntraUser=true, Person exists.
    /// </summary>
    public async Task<PersonEntity?> GetEntraLinkedPersonAsync(
        Guid entraObjectId, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntities
            .AsNoTracking()
            .Where(be => be.Rowguid == entraObjectId && be.IsEntraUser == true)
            .SelectMany(be => be.Persons)
            .Include(p => p.BusinessEntity)
            .Include(p => p.PersonType)
            .Include(p => p.EmailAddresses)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
