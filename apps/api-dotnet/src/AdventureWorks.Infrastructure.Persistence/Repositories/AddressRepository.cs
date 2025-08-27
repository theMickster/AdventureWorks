using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class AddressRepository(AdventureWorksDbContext dbContext)
    : EfRepository<AddressEntity>(dbContext), IAddressRepository
{
    public async Task<AddressEntity?> GetAddressByIdAsync(int addressId)
    {
        return await DbContext.Addresses

            .Include( a => a.StateProvince)
            .ThenInclude(b => b.CountryRegion)
            .Where(x => x.AddressId == addressId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns true if an address with the given id exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int addressId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Addresses.AnyAsync(x => x.AddressId == addressId, cancellationToken);
    }
}
