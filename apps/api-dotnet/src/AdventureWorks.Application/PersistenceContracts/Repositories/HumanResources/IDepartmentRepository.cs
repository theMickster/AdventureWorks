using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;

public interface IDepartmentRepository : IAsyncRepository<DepartmentEntity>
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

    /// <summary>Returns true if a department with the given name already exists.</summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Returns true if a department with the given name already exists, excluding the specified department ID.</summary>
    Task<bool> ExistsByNameExcludingIdAsync(string name, short excludeId, CancellationToken cancellationToken = default);
}
