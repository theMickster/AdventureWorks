using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.HumanResources;

[ServiceLifetimeScoped]
public sealed class DepartmentRepository : ReadOnlyEfRepository<DepartmentEntity>, IDepartmentRepository
{
    public DepartmentRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<IReadOnlyList<DepartmentEntity>> ListAllAsync()
    {
        return await DbContext.Departments
            .ToListAsync();
    }

    /// <summary>
    /// Retrieve a department entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task<DepartmentEntity> GetByIdAsync(int id)
    {
        return await DbContext.Departments
            .FirstOrDefaultAsync(s => s.DepartmentId == id);
    }
}
