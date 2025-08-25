using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for BusinessEntityContact junction entity operations.
/// The composite primary key is (BusinessEntityId, PersonId, ContactTypeId).
/// </summary>
public interface IBusinessEntityContactEntityRepository : IAsyncRepository<BusinessEntityContactEntity>
{
    /// <summary>
    /// Retrieve the list of business contacts for a given store (business entity) id
    /// </summary>
    /// <param name="businessEntityId">the unique business entity (store) identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    Task<List<BusinessEntityContactEntity>> GetContactsByIdAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of business contacts for a list of store (business entity) ids
    /// </summary>
    /// <param name="businessEntityIds">the list of business entity (store) identifiers</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns></returns>
    Task<List<BusinessEntityContactEntity>> GetContactsByStoreIdsAsync(List<int> businessEntityIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a contact with the given composite key exists.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="personId">the unique person identifier</param>
    /// <param name="contactTypeId">the unique contact type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracked contact by its composite key.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="personId">the unique person identifier</param>
    /// <param name="contactTypeId">the unique contact type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<BusinessEntityContactEntity?> GetByCompositeKeyAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a contact (with ContactType and Person details) by its composite key. Read-only.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="personId">the unique person identifier</param>
    /// <param name="contactTypeId">the unique contact type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<BusinessEntityContactEntity?> GetWithDetailsByCompositeKeyAsync(int storeId, int personId, int contactTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing contact's contact type by deleting the existing row and inserting a new one
    /// (the composite primary key includes <c>ContactTypeId</c>, so a true update is not possible).
    /// Performed inside a single database transaction.
    /// </summary>
    /// <param name="existing">the tracked contact entity to be replaced</param>
    /// <param name="newContactTypeId">the new contact type identifier</param>
    /// <param name="modifiedDate">timestamp to apply to the replacement row</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<BusinessEntityContactEntity> ReplaceContactTypeAsync(BusinessEntityContactEntity existing, int newContactTypeId, DateTime modifiedDate, CancellationToken cancellationToken = default);
}
