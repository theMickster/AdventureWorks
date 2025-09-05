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

    /// <summary>
    /// Returns the count of active employees currently assigned to a department (EndDate IS NULL and CurrentFlag is true).
    /// </summary>
    public async Task<int> GetDepartmentHeadcountAsync(
        short departmentId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.EmployeeDepartmentHistories
            .AsNoTracking()
            .CountAsync(edh => edh.DepartmentId == departmentId
                            && edh.EndDate == null
                            && edh.BusinessEntity.CurrentFlag,
                        cancellationToken);
    }

    /// <summary>
    /// Returns all departments with their active employee counts. Departments with zero active employees are included.
    /// Uses two queries assembled in memory to avoid a complex correlated subquery.
    /// </summary>
    public async Task<IReadOnlyList<(DepartmentEntity Dept, int Count)>> GetDepartmentHeadcountSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var activeCounts = await DbContext.EmployeeDepartmentHistories
            .AsNoTracking()
            .Where(edh => edh.EndDate == null && edh.BusinessEntity.CurrentFlag)
            .GroupBy(edh => edh.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var departments = await DbContext.Departments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var countMap = activeCounts.ToDictionary(x => x.DepartmentId, x => x.Count);
        return departments
            .Select(d => (Dept: d, Count: countMap.TryGetValue(d.DepartmentId, out var c) ? c : 0))
            .OrderByDescending(x => x.Count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns a paginated list of active employees currently assigned to a department, ordered by LastName then FirstName.
    /// </summary>
    public async Task<(IReadOnlyList<EmployeeEntity> Employees, int TotalCount)> GetEmployeesByDepartmentAsync(
        short departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Employees
            .AsNoTracking()
            .Include(e => e.PersonBusinessEntity)
                .ThenInclude(p => p.EmailAddresses)
            .Where(e => e.CurrentFlag
                     && e.EmployeeDepartmentHistory.Any(
                         edh => edh.DepartmentId == departmentId && edh.EndDate == null))
            .OrderBy(e => e.PersonBusinessEntity.LastName)
            .ThenBy(e => e.PersonBusinessEntity.FirstName);

        var totalCount = await query.CountAsync(cancellationToken);
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (employees.AsReadOnly(), totalCount);
    }
}
