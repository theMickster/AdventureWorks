using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

/// <summary>
/// Repository for Person entity operations with Entra user validation.
/// </summary>
[ServiceLifetimeScoped]
public sealed class PersonRepository(AdventureWorksDbContext dbContext)
    : ReadOnlyEfRepository<PersonEntity>(dbContext), IPersonRepository
{
    /// <summary>
    /// Retrieves a person and supporting detail graph by business entity id.
    /// </summary>
    /// <param name="personId">the person business entity id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<PersonEntity?> GetPersonDetailByIdAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Persons
            .AsNoTracking()
            .Include(x => x.PersonType)
            .Include(x => x.EmailAddresses)
            .Include(x => x.PersonPhones)
                .ThenInclude(x => x.PhoneNumberType)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == personId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a Person entity linked to an Entra user with validation.
    /// Performs checks: BusinessEntity.Rowguid matches, IsEntraUser=true, Person exists.
    /// </summary>
    public async Task<PersonEntity?> GetEntraLinkedPersonAsync(
        Guid entraObjectId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntities
            .AsNoTracking()
            .Where(be => be.Rowguid == entraObjectId && be.IsEntraUser == true)
            .SelectMany(be => be.Persons)
            .Include(p => p.BusinessEntity)
            .Include(p => p.PersonType)
            .Include(p => p.EmailAddresses)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Returns true if a person with the given id exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Persons.AnyAsync(x => x.BusinessEntityId == id, cancellationToken);
    }

    /// <summary>
    /// Searches for persons using optional filters with pagination.
    /// Applies AND logic across filter types and OR logic within partial name matches.
    /// Escapes LIKE wildcard characters (%, _, [, ]) to prevent SQL injection.
    /// </summary>
    public async Task<(IEnumerable<PersonEntity> Persons, int TotalCount)> SearchAsync(
        string? firstName,
        string? lastName,
        string? personTypeCode,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PersonEntity> query = DbContext.Persons
            .AsNoTracking()
            .Include(x => x.PersonType)
            .Include(x => x.EmailAddresses);

        // Apply firstName filter (partial match with escaped wildcard characters)
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            var escapedFirstName = EscapeLikeMagicChars(firstName);
            query = query.Where(x => EF.Functions.Like(x.FirstName, $"%{escapedFirstName}%", "\\"));
        }

        // Apply lastName filter (partial match with escaped wildcard characters)
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            var escapedLastName = EscapeLikeMagicChars(lastName);
            query = query.Where(x => EF.Functions.Like(x.LastName, $"%{escapedLastName}%", "\\"));
        }

        // Apply personTypeCode filter (exact match)
        if (!string.IsNullOrWhiteSpace(personTypeCode))
        {
            query = query.Where(x => x.PersonType != null && x.PersonType.PersonTypeCode == personTypeCode);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination: skip = (page - 1) * pageSize, take = pageSize
        var skip = (page - 1) * pageSize;
        var persons = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (persons, totalCount);
    }

    /// <summary>
    /// Escapes LIKE magic characters (%, _, [, ]) in the input string to prevent SQL injection.
    /// </summary>
    /// <param name="input">The input string to escape.</param>
    /// <returns>The escaped string with wildcard characters escaped by backslash.</returns>
    private static string EscapeLikeMagicChars(string input)
    {
        return input
            .Replace("\\", "\\\\")  // Escape backslash first
            .Replace("%", "\\%")    // Escape percent
            .Replace("_", "\\_")    // Escape underscore
            .Replace("[", "\\[");   // Escape left bracket
    }
}
