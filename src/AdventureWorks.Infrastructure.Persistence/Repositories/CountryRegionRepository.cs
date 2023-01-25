using System.Linq;
using System.Threading.Tasks;
using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public sealed class CountryRegionRepository : ReadOnlyEfRepository<CountryRegionEntity>, ICountryRegionRepository
{
    public CountryRegionRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Retrieve a country region entity using its unique code
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CountryRegionEntity> GetByIdAsync(string id)
    {
        return await DbContext
            .CountryRegions
            .FirstOrDefaultAsync(x => x.CountryRegionCode == id);
    }
}