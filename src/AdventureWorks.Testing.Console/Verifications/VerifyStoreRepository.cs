using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Application.Interfaces.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorks.Testing.Console.Verifications;
internal sealed class VerifyStoreRepository
{
    private readonly IAdventureWorksDbContext _dbContext;
    private readonly IStoreRepository _repository;

    public VerifyStoreRepository(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

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

        var result = _dbContext
            .Stores
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(y => y.AddressType)
            .Include(x => x.StoreBusinessEntity)
            .ThenInclude(y => y.BusinessEntityAddresses)
            .ThenInclude(z => z.Address)
            .FirstOrDefault(x => x.BusinessEntityId == 644);

        if (result is null)
        {
            success = false;
        }

        result = await _repository.GetByIdAsync(644).ConfigureAwait(false);

        if (result is null)
        {
            success = false;
        }

        return (success, errorList);
    }

}
