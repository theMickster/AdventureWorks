using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class PersonPhoneRepository(AdventureWorksDbContext dbContext)
    : EfRepository<PersonPhone>(dbContext), IPersonPhoneRepository
{
    /// <summary>
    /// Retrieves all phone numbers for the specified person including PhoneNumberType. Read-only.
    /// </summary>
    public async Task<List<PersonPhone>> GetPhonesByPersonIdAsync(int businessEntityId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PersonPhones
            .Include(x => x.PhoneNumberType)
            .Where(x => x.BusinessEntityId == businessEntityId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a tracked phone by (BusinessEntityId, PhoneNumberTypeId). Returns null when not found.
    /// </summary>
    public async Task<PersonPhone?> GetTrackedPhoneAsync(int businessEntityId, int phoneNumberTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PersonPhones
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == businessEntityId && x.PhoneNumberTypeId == phoneNumberTypeId,
                cancellationToken);
    }

    /// <summary>
    /// Retrieves a phone (with PhoneNumberType) by its full composite key. Read-only.
    /// </summary>
    public async Task<PersonPhone?> GetPhoneWithDetailsByCompositeKeyAsync(int businessEntityId, string phoneNumber, int phoneNumberTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PersonPhones
            .Include(x => x.PhoneNumberType)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == businessEntityId && x.PhoneNumber == phoneNumber && x.PhoneNumberTypeId == phoneNumberTypeId,
                cancellationToken);
    }

    /// <summary>
    /// Returns true if a Person record with the given BusinessEntityId exists.
    /// </summary>
    public async Task<bool> PersonExistsAsync(int businessEntityId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Persons.AnyAsync(x => x.BusinessEntityId == businessEntityId, cancellationToken);
    }

    /// <summary>
    /// Returns true if a PhoneNumberType with the given id exists.
    /// </summary>
    public async Task<bool> PhoneNumberTypeExistsAsync(int phoneNumberTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PhoneNumberTypes.AnyAsync(x => x.PhoneNumberTypeId == phoneNumberTypeId, cancellationToken);
    }

    /// <summary>
    /// Returns true if the (BusinessEntityId, PhoneNumber, PhoneNumberTypeId) combination already exists.
    /// </summary>
    public async Task<bool> PhoneCombinationExistsAsync(int businessEntityId, string phoneNumber, int phoneNumberTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PersonPhones
            .AnyAsync(
                x => x.BusinessEntityId == businessEntityId && x.PhoneNumber == phoneNumber && x.PhoneNumberTypeId == phoneNumberTypeId,
                cancellationToken);
    }

    /// <summary>
    /// Replaces an existing phone number by deleting the existing row and inserting a new one inside a
    /// single transaction. Required because PhoneNumber is part of the composite primary key.
    /// </summary>
    public async Task<PersonPhone> ReplacePhoneAsync(
        PersonPhone existing,
        string newPhoneNumber,
        DateTime modifiedDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(existing);

        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            DbContext.PersonPhones.Remove(existing);
            await DbContext.SaveChangesAsync(cancellationToken);

            var replacement = new PersonPhone
            {
                BusinessEntityId = existing.BusinessEntityId,
                PhoneNumber = newPhoneNumber,
                PhoneNumberTypeId = existing.PhoneNumberTypeId,
                ModifiedDate = modifiedDate
            };

            DbContext.PersonPhones.Add(replacement);
            await DbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return replacement;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Hard-deletes the phone identified by (BusinessEntityId, PhoneNumberTypeId).
    /// </summary>
    public async Task DeletePhoneAsync(int businessEntityId, int phoneNumberTypeId, CancellationToken cancellationToken = default)
    {
        var entity = await DbContext.PersonPhones
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == businessEntityId && x.PhoneNumberTypeId == phoneNumberTypeId,
                cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Phone number with type {phoneNumberTypeId} not found for person {businessEntityId}.");
        }

        DbContext.PersonPhones.Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
