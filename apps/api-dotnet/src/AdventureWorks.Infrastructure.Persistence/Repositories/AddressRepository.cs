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
}
