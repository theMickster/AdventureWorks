using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.Interfaces;

public interface IProductRepository : IAsyncRepository<Product>
{
    Task<Product> GetByIdWithItemsAsync(int id);

    Task<List<Product>> GetAllProductsAsync();

}