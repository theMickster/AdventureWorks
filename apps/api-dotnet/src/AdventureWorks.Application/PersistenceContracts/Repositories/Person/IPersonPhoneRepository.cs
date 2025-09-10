using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for PersonPhone entity operations.
/// The composite primary key is (BusinessEntityId, PhoneNumber, PhoneNumberTypeId).
/// </summary>
public interface IPersonPhoneRepository : IAsyncRepository<PersonPhone>
{
    /// <summary>
    /// Retrieves all phone numbers for the specified person including PhoneNumberType. Read-only.
    /// </summary>
    Task<List<PersonPhone>> GetPhonesByPersonIdAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracked phone by (BusinessEntityId, PhoneNumberTypeId). Returns null when not found.
    /// Tracking is required for delete and replace operations.
    /// </summary>
    Task<PersonPhone?> GetTrackedPhoneAsync(int businessEntityId, int phoneNumberTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a phone (with PhoneNumberType) by its full composite key. Read-only.
    /// Called after ReplacePhoneAsync to re-hydrate the nav property before mapping.
    /// </summary>
    Task<PersonPhone?> GetPhoneWithDetailsByCompositeKeyAsync(int businessEntityId, string phoneNumber, int phoneNumberTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a Person record with the given BusinessEntityId exists.
    /// </summary>
    Task<bool> PersonExistsAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a PhoneNumberType with the given id exists.
    /// </summary>
    Task<bool> PhoneNumberTypeExistsAsync(int phoneNumberTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the (BusinessEntityId, PhoneNumber, PhoneNumberTypeId) combination already exists.
    /// </summary>
    Task<bool> PhoneCombinationExistsAsync(int businessEntityId, string phoneNumber, int phoneNumberTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing phone number by deleting the existing row and inserting a new one inside a
    /// single transaction. Required because PhoneNumber is part of the composite primary key.
    /// </summary>
    Task<PersonPhone> ReplacePhoneAsync(PersonPhone existing, string newPhoneNumber, DateTime modifiedDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard-deletes the phone identified by (BusinessEntityId, PhoneNumberTypeId).
    /// </summary>
    Task DeletePhoneAsync(int businessEntityId, int phoneNumberTypeId, CancellationToken cancellationToken = default);
}
