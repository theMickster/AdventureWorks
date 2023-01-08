using System.Linq;
using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdventureWorks.Infrastructure.Repositories;
public sealed class AddressRepository : EfRepository<AddressEntity>, IAddressRepository
{
    public AddressRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {

    }

    public async Task<AddressEntity> GetAddressByIdAsync(int addressId)
    {
        return await _dbContext.Addresses
            
            .Include( a => a.StateProvince)
            .ThenInclude(b => b.CountryRegion)
            .Where(x => x.AddressId == addressId)
            .FirstOrDefaultAsync();
    }
}
