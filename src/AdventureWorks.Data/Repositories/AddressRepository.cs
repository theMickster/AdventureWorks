﻿using System.Linq;
using System.Threading.Tasks;
using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;
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
