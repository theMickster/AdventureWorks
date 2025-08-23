using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class PhoneNumberTypeRepository(AdventureWorksDbContext dbContext)
    : ReadOnlyEfRepository<PhoneNumberTypeEntity>(dbContext), IPhoneNumberTypeRepository
{
    public override async Task<IReadOnlyList<PhoneNumberTypeEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.PhoneNumberTypes
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieve a phone number type entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<PhoneNumberTypeEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.PhoneNumberTypes
            .FirstOrDefaultAsync(s => s.PhoneNumberTypeId == id, cancellationToken);
    }
}
