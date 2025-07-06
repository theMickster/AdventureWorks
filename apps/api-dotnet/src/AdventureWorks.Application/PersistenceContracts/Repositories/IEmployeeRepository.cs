using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

/// <summary>
/// Repository contract for Employee entity operations.
/// </summary>
public interface IEmployeeRepository : IAsyncRepository<EmployeeEntity>
{
    /// <summary>
    /// Creates a new employee with full entity graph: BusinessEntity → Person → Employee.
    /// Leverages EF Core navigation properties for cascade inserts.
    /// All operations are performed within a single transaction.
    /// </summary>
    /// <param name="employeeEntity">Employee entity with navigation properties populated</param>
    /// <param name="personEntity">Person entity to be created</param>
    /// <param name="personPhone">Optional phone number entity</param>
    /// <param name="emailAddress">Optional email address entity</param>
    /// <param name="address">Optional address entity</param>
    /// <param name="addressTypeId">Address type ID (required if address provided)</param>
    /// <param name="modifiedDate">System modification timestamp</param>
    /// <param name="rowGuid">System-generated unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>BusinessEntityId of the created employee</returns>
    Task<int> CreateEmployeeWithPersonAsync(
        EmployeeEntity employeeEntity,
        PersonEntity personEntity,
        PersonPhone personPhone,
        EmailAddressEntity emailAddress,
        AddressEntity address,
        int addressTypeId,
        DateTime modifiedDate,
        Guid rowGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an employee by their BusinessEntityId with Person data.
    /// </summary>
    /// <param name="businessEntityId">The unique business entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee entity or null if not found</returns>
    Task<EmployeeEntity?> GetEmployeeByIdAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of employees and the total count of employees in the database.
    /// </summary>
    /// <param name="parameters">The input paging parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing the list of employees and total count</returns>
    Task<(IReadOnlyList<EmployeeEntity>, int)> GetEmployeesAsync(EmployeeParameter parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of employees filtered using the search criteria.
    /// </summary>
    /// <param name="parameters">The input paging parameters</param>
    /// <param name="employeeSearchModel">The employee search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing the filtered list of employees and total count</returns>
    Task<(IReadOnlyList<EmployeeEntity>, int)> SearchEmployeesAsync(
        EmployeeParameter parameters,
        EmployeeSearchModel employeeSearchModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all addresses for a specific employee.
    /// </summary>
    /// <param name="businessEntityId">The employee's business entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of BusinessEntityAddress entities with Address and AddressType included</returns>
    Task<IReadOnlyList<BusinessEntityAddressEntity>> GetEmployeeAddressesAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific address for an employee.
    /// </summary>
    /// <param name="businessEntityId">The employee's business entity identifier</param>
    /// <param name="addressId">The address identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>BusinessEntityAddress entity with Address and AddressType included, or null if not found</returns>
    Task<BusinessEntityAddressEntity?> GetEmployeeAddressByIdAsync(int businessEntityId, int addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an employee by their BusinessEntityId with EmployeeDepartmentHistory collection.
    /// Includes related Department and Shift entities for each history record.
    /// Used by lifecycle commands that need to manage department assignments.
    /// </summary>
    /// <param name="businessEntityId">The unique business entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee entity with department history, or null if not found</returns>
    Task<EmployeeEntity?> GetEmployeeByIdWithDepartmentHistoryAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an employee by their BusinessEntityId with full lifecycle data.
    /// Includes Person, EmployeeDepartmentHistory (with Department and Shift), and EmployeePayHistory.
    /// Used by lifecycle status query to aggregate comprehensive employee information.
    /// </summary>
    /// <param name="businessEntityId">The unique business entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee entity with full lifecycle data, or null if not found</returns>
    Task<EmployeeEntity?> GetEmployeeByIdWithLifecycleDataAsync(int businessEntityId, CancellationToken cancellationToken = default);
}
