using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class AddressTypeRepository : ReadOnlyEfRepository<AddressTypeEntity>, IAddressTypeRepository
{
    public AddressTypeRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns true if an address type with the given id exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int addressTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.AddressTypes.AnyAsync(x => x.AddressTypeId == addressTypeId, cancellationToken);
    }
}
