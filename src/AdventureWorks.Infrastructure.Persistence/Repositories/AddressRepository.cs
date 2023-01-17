using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class AddressRepository : EfRepository<AddressEntity>, IAddressRepository
{
    public AddressRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {

    }

    public async Task<AddressEntity> GetAddressByIdAsync(int addressId)
    {
        return await DbContext.Addresses
            
            .Include( a => a.StateProvince)
            .ThenInclude(b => b.CountryRegion)
            .Where(x => x.AddressId == addressId)
            .FirstOrDefaultAsync();
    }
}
