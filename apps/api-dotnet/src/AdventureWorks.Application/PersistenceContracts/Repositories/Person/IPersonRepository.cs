using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for Person entity operations.
/// </summary>
public interface IPersonRepository : IReadOnlyAsyncRepository<PersonEntity>
{
    /// <summary>
    /// Retrieves a person by BusinessEntityId with related details for consolidated person reads.
    /// Includes PersonType, EmailAddresses, and PersonPhones with PhoneNumberType.
    /// </summary>
    /// <param name="personId">the person business entity id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<PersonEntity?> GetPersonDetailByIdAsync(int personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Person entity that is linked to a Microsoft Entra ID user.
    /// Validates that the BusinessEntity exists, IsEntraUser=true, and Person record exists.
    /// </summary>
    /// <param name="entraObjectId">The Microsoft Entra Object ID (oid claim GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// PersonEntity with BusinessEntity, PersonType, and EmailAddresses navigation properties included,
    /// or null if any validation check fails.
    /// </returns>
    Task<PersonEntity?> GetEntraLinkedPersonAsync(
        Guid entraObjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a person with the given id exists.
    /// </summary>
    /// <param name="id">the person business entity id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
