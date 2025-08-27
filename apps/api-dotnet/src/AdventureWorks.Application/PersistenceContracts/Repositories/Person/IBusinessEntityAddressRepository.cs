using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for BusinessEntityAddress junction entity operations.
/// The composite primary key is (BusinessEntityId, AddressId, AddressTypeId).
/// </summary>
public interface IBusinessEntityAddressRepository : IAsyncRepository<BusinessEntityAddressEntity>
{
    /// <summary>
    /// Retrieves the list of addresses (with AddressType, Address, StateProvince, and CountryRegion details) for a given store (business entity) id. Read-only.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>List of business entity address entities for the store, or an empty list if none exist.</returns>
    Task<List<BusinessEntityAddressEntity>> GetAddressesByStoreIdAsync(int storeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if an address with the given composite key exists.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="addressId">the unique address identifier</param>
    /// <param name="addressTypeId">the unique address type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int storeId, int addressId, int addressTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracked address by its composite key.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="addressId">the unique address identifier</param>
    /// <param name="addressTypeId">the unique address type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<BusinessEntityAddressEntity?> GetByCompositeKeyAsync(int storeId, int addressId, int addressTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an address (with AddressType, Address, StateProvince, and CountryRegion details) by its composite key. Read-only.
    /// </summary>
    /// <param name="storeId">the unique business entity (store) identifier</param>
    /// <param name="addressId">the unique address identifier</param>
    /// <param name="addressTypeId">the unique address type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<BusinessEntityAddressEntity?> GetWithDetailsByCompositeKeyAsync(int storeId, int addressId, int addressTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing address's address type by deleting the existing row and inserting a new one
    /// (the composite primary key includes <c>AddressTypeId</c>, so a true update is not possible).
    /// Performed inside a single database transaction.
    /// </summary>
    /// <param name="existing">the tracked address entity to be replaced</param>
    /// <param name="newAddressTypeId">the new address type identifier</param>
    /// <param name="modifiedDate">timestamp to apply to the replacement row</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<BusinessEntityAddressEntity> ReplaceAddressTypeAsync(BusinessEntityAddressEntity existing, int newAddressTypeId, DateTime modifiedDate, CancellationToken cancellationToken = default);
}
