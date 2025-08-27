using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Person;

[ServiceLifetimeScoped]
public sealed class BusinessEntityAddressRepository : EfRepository<BusinessEntityAddressEntity>, IBusinessEntityAddressRepository
{
    public BusinessEntityAddressRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {

    }

    /// <summary>
    /// Retrieves the list of addresses (with AddressType, Address, StateProvince, and CountryRegion details) for a given store id. Read-only.
    /// </summary>
    public async Task<List<BusinessEntityAddressEntity>> GetAddressesByStoreIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityAddresses
            .AsNoTracking()
            .Where(x => x.BusinessEntityId == storeId)
            .Include(x => x.AddressType)
            .Include(x => x.Address)
                .ThenInclude(a => a.StateProvince)
                .ThenInclude(s => s.CountryRegion)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns true if an address with the given composite key exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int storeId, int addressId, int addressTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityAddresses.AnyAsync(
            x => x.BusinessEntityId == storeId && x.AddressId == addressId && x.AddressTypeId == addressTypeId,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves a tracked address by its composite key. Returns null when not found.
    /// </summary>
    public async Task<BusinessEntityAddressEntity?> GetByCompositeKeyAsync(int storeId, int addressId, int addressTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityAddresses.FirstOrDefaultAsync(
            x => x.BusinessEntityId == storeId && x.AddressId == addressId && x.AddressTypeId == addressTypeId,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves an address (with AddressType, Address, StateProvince, and CountryRegion details) by its composite key. Read-only.
    /// </summary>
    public async Task<BusinessEntityAddressEntity?> GetWithDetailsByCompositeKeyAsync(int storeId, int addressId, int addressTypeId, CancellationToken cancellationToken = default)
    {
        return await DbContext.BusinessEntityAddresses
            .AsNoTracking()
            .Include(x => x.AddressType)
            .Include(x => x.Address)
                .ThenInclude(a => a.StateProvince)
                .ThenInclude(s => s.CountryRegion)
            .FirstOrDefaultAsync(
                x => x.BusinessEntityId == storeId && x.AddressId == addressId && x.AddressTypeId == addressTypeId,
                cancellationToken);
    }

    /// <summary>
    /// Replaces an existing address's address type by deleting the existing row and inserting a new one,
    /// inside a single transaction. Required because the composite primary key includes AddressTypeId.
    /// </summary>
    public async Task<BusinessEntityAddressEntity> ReplaceAddressTypeAsync(
        BusinessEntityAddressEntity existing,
        int newAddressTypeId,
        DateTime modifiedDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(existing);

        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            DbContext.BusinessEntityAddresses.Remove(existing);
            await DbContext.SaveChangesAsync(cancellationToken);

            var replacement = new BusinessEntityAddressEntity
            {
                BusinessEntityId = existing.BusinessEntityId,
                AddressId = existing.AddressId,
                AddressTypeId = newAddressTypeId,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate
            };

            DbContext.BusinessEntityAddresses.Add(replacement);
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
}
