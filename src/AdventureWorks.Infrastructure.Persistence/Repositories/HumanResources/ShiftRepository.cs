using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.HumanResources;

[ServiceLifetimeScoped]
public sealed class ShiftRepository : ReadOnlyEfRepository<ShiftEntity>, IShiftRepository
{
    public ShiftRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<IReadOnlyList<ShiftEntity>> ListAllAsync()
    {
        return await DbContext.Shifts
            .ToListAsync();
    }

    /// <summary>
    /// Retrieve a shift entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task<ShiftEntity> GetByIdAsync(int id)
    {
        return await DbContext.Shifts
            .FirstOrDefaultAsync(s => s.ShiftId == id);
    }
}
