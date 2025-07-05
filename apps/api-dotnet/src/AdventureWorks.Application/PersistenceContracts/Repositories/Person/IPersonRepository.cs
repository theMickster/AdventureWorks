using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for Person entity operations.
/// </summary>
public interface IPersonRepository : IReadOnlyAsyncRepository<PersonEntity>
{
    /// <summary>
    /// Retrieves a Person entity that is linked to a Microsoft Entra ID user.
    /// Validates that the BusinessEntity exists, IsEntraUser=true, and Person record exists.
    /// </summary>
    /// <param name="entraObjectId">The Microsoft Entra Object ID (oid claim GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// PersonEntity with BusinessEntity and PersonType navigation properties included,
    /// or null if any validation check fails.
    /// </returns>
    Task<PersonEntity?> GetEntraLinkedPersonAsync(
        Guid entraObjectId, 
        CancellationToken cancellationToken = default);
}
