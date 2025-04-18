using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface IProductRepository : IAsyncRepository<Product>
{
    Task<Product> GetByIdWithItemsAsync(int id);

    Task<List<Product>> GetAllProductsAsync();

}