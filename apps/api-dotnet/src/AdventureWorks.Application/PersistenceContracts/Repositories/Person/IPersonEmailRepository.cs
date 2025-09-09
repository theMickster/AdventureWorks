using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for PersonEmail (EmailAddress) entity operations.
/// The composite primary key is (BusinessEntityId, EmailAddressId).
/// </summary>
public interface IPersonEmailRepository : IAsyncRepository<EmailAddressEntity>
{
    /// <summary>
    /// Retrieves all email addresses for the specified person. Read-only.
    /// </summary>
    /// <param name="businessEntityId">The person's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<List<EmailAddressEntity>> GetEmailsByPersonIdAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracked email address by its composite key (BusinessEntityId + EmailAddressId).
    /// Returns null when not found. Tracking is required for update operations.
    /// </summary>
    /// <param name="businessEntityId">The person's BusinessEntityId.</param>
    /// <param name="emailAddressId">The email address identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<EmailAddressEntity?> GetEmailByCompositeKeyAsync(int businessEntityId, int emailAddressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a Person record with the given BusinessEntityId exists.
    /// </summary>
    /// <param name="businessEntityId">The person's BusinessEntityId.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> PersonExistsAsync(int businessEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if an email address (case-insensitive) already exists for the specified person.
    /// Used for duplicate detection on create and update.
    /// </summary>
    /// <param name="businessEntityId">The person's BusinessEntityId.</param>
    /// <param name="emailAddress">The email address to check.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<bool> EmailExistsForPersonAsync(int businessEntityId, string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard-deletes the email address identified by the composite key.
    /// </summary>
    /// <param name="businessEntityId">The person's BusinessEntityId.</param>
    /// <param name="emailAddressId">The email address identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteEmailAsync(int businessEntityId, int emailAddressId, CancellationToken cancellationToken = default);
}
