using AdventureWorks.Application.PersistenceContracts.DbContext;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Testing.Console.Verifications;
internal sealed class VerifyStoreRepository
{
    private readonly IAdventureWorksDbContext _dbContext;
    private readonly IStoreRepository _repository;

    public VerifyStoreRepository(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _dbContext = serviceProvider.GetRequiredService<IAdventureWorksDbContext>() ??
                     throw new InvalidOperationException(
                         "Unable to find a concrete implementation of IAdventureWorksDbContext ");

        _repository = serviceProvider.GetRequiredService<IStoreRepository>() ??
                      throw new InvalidOperationException(
                          "Unable to find a concrete implementation of IStoreRepository ");
    }

    public async Task<(bool status, List<string> errorsList)> VerifyAllTheThings()
    {
        var success = true;
        var errorList = new List<string>();

        var result = await _dbContext
            .Stores
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(z => z.Address)
            .FirstOrDefaultAsync(x => x.BusinessEntityId == 644);

        if (result is null)
        {
            success = false;
        }

        result = await _repository.GetByIdAsync(644);

        if (result is null)
        {
            success = false;
        }

        result = await _repository.GetStoreByIdAsync(644);


        return (success, errorList);
    }

}
