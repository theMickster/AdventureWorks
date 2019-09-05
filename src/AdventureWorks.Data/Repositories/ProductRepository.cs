using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdventureWorks.Application.Interfaces;
using AdventureWorks.Core.Entities;
using AdventureWorks.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Repositories
{
    public class ProductRepository : EfRepository<Product>, IProductRepository
    {
        public ProductRepository(AdventureWorksDbContext dbContext) : base(dbContext)
        {
        }

        public Task<Product> GetByIdWithItemsAsync(int id)
        {
            return _dbContext.Products
                .Include(p => p.ProductModel)
                .Include(p=> p.ProductSubcategory)
                .FirstOrDefaultAsync(x => x.ProductId == id);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }
    }
}
