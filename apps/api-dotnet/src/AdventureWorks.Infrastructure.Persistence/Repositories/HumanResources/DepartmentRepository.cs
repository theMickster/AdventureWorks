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

    public override async Task<IReadOnlyList<DepartmentEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Departments
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieve a department entity by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<DepartmentEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Departments
            .FirstOrDefaultAsync(s => s.DepartmentId == id, cancellationToken);
    }
}
