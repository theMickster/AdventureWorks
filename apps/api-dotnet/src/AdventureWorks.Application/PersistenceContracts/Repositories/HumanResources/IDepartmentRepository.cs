using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;

public interface IDepartmentRepository : IReadOnlyAsyncRepository<DepartmentEntity>
{
    /// <summary>Returns the count of active employees with an open assignment in the specified department.</summary>
    Task<int> GetDepartmentHeadcountAsync(
        short departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all departments with their active employee counts, ordered by count descending. Zero-count departments are included.</summary>
    Task<IReadOnlyList<(DepartmentEntity Dept, int Count)>> GetDepartmentHeadcountSummaryAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated list of active employees with an open assignment in the specified department, ordered by LastName then FirstName.</summary>
    Task<(IReadOnlyList<EmployeeEntity> Employees, int TotalCount)> GetEmployeesByDepartmentAsync(
        short departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
