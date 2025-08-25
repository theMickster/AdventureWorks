using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Person;

/// <summary>
/// Repository contract for ContactType entity read operations.
/// </summary>
public interface IContactTypeEntityRepository : IReadOnlyAsyncRepository<ContactTypeEntity>
{
    /// <summary>
    /// Returns true if a contact type with the given id exists.
    /// </summary>
    /// <param name="id">the contact type identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
