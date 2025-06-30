using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

[ServiceLifetimeScoped]
public class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(AdventureWorksDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Product> GetByIdWithItemsAsync(int id)
    {
        return DbContext.Products
            .Include(p => p.ProductModel)
            .Include(p=> p.ProductSubcategory)
            .FirstOrDefaultAsync(x => x.ProductId == id);
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await DbContext.Products.ToListAsync();
    }
}