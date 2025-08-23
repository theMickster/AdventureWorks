using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Common.Helpers;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Production;

[ServiceLifetimeScoped]
public sealed class ProductRepository(AdventureWorksDbContext dbContext)
    : EfRepository<Product>(dbContext), IProductRepository
{

    /// <summary>
    /// Retrieve a product by id along with its related entities
    /// </summary>
    /// <param name="productId">the unique product identifier</param>
    public async Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products
            .AsNoTracking()
            .Include(p => p.ProductSubcategory)
                .ThenInclude(sc => sc.ProductCategory)
            .Include(p => p.ProductModel)
            .Include(p => p.ProductProductPhotos)
                .ThenInclude(pp => pp.ProductPhoto)
            .Include(p => p.ProductInventory)
                .ThenInclude(pi => pi.Location)
            .Include(p => p.SizeUnitMeasureCodeNavigation)
            .Include(p => p.WeightUnitMeasureCodeNavigation)
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated list of products and the total count of products in the database.
    /// </summary>
    /// <param name="parameters">the input paging parameters</param>
    public async Task<(IReadOnlyList<Product>, int)> GetProductsAsync(ProductParameter parameters, CancellationToken cancellationToken = default)
    {
        var query = BuildProductListQuery();

        query = ApplySorting(query, parameters);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await query.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves a paged list of products that is filtered using the <paramref name="searchModel"/> input parameter.
    /// </summary>
    public async Task<(IReadOnlyList<Product>, int)> SearchProductsAsync(
        ProductParameter parameters,
        ProductSearchModel searchModel,
        CancellationToken cancellationToken = default)
    {
        var query = BuildProductListQuery();

        if (searchModel != null)
        {
            if (searchModel.Id != null)
            {
                query = query.Where(p => p.ProductId == searchModel.Id);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                var pattern = $"%{EscapeLikePattern(searchModel.Name.Trim())}%";
                query = query.Where(p => EF.Functions.Like(p.Name, pattern));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ProductNumber))
            {
                var pattern = $"%{EscapeLikePattern(searchModel.ProductNumber.Trim())}%";
                query = query.Where(p => EF.Functions.Like(p.ProductNumber, pattern));
            }

            if (searchModel.CategoryId != null)
            {
                query = query.Where(p => p.ProductSubcategory != null
                    && p.ProductSubcategory.ProductCategoryId == searchModel.CategoryId);
            }

            if (searchModel.SubcategoryId != null)
            {
                query = query.Where(p => p.ProductSubcategoryId == searchModel.SubcategoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Color))
            {
                var pattern = $"%{EscapeLikePattern(searchModel.Color.Trim())}%";
                query = query.Where(p => p.Color != null
                    && EF.Functions.Like(p.Color, pattern));
            }

            if (searchModel.MinListPrice != null)
            {
                query = query.Where(p => p.ListPrice >= searchModel.MinListPrice);
            }

            if (searchModel.MaxListPrice != null)
            {
                query = query.Where(p => p.ListPrice <= searchModel.MaxListPrice);
            }

            if (searchModel.IsActive != null)
            {
                query = searchModel.IsActive.Value
                    ? query.Where(p => p.DiscontinuedDate == null)
                    : query.Where(p => p.DiscontinuedDate != null);
            }
        }

        query = ApplySorting(query, parameters);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await query.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves all product categories.
    /// </summary>
    public async Task<IReadOnlyList<ProductCategory>> GetProductCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var results = await DbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return results.AsReadOnly();
    }

    /// <summary>
    /// Retrieves product subcategories, optionally filtered by category.
    /// </summary>
    public async Task<IReadOnlyList<ProductSubcategory>> GetProductSubcategoriesAsync(int? categoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbContext.ProductSubcategories
            .AsNoTracking()
            .Include(s => s.ProductCategory)
            .AsQueryable();

        if (categoryId != null)
        {
            query = query.Where(s => s.ProductCategoryId == categoryId);
        }

        var results = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);

        return results.AsReadOnly();
    }

    /// <summary>
    /// Retrieves inventory records for a specific product.
    /// </summary>
    public async Task<IReadOnlyList<ProductInventory>> GetProductInventoryByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var results = await DbContext.ProductInventories
            .AsNoTracking()
            .Include(pi => pi.Location)
            .Where(pi => pi.ProductId == productId)
            .ToListAsync(cancellationToken);

        return results.AsReadOnly();
    }

    /// <summary>
    /// Retrieves list price history records for a specific product.
    /// </summary>
    public async Task<IReadOnlyList<ProductListPriceHistory>> GetListPriceHistoryByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var results = await DbContext.ProductListPriceHistories
            .AsNoTracking()
            .Where(h => h.ProductId == productId)
            .OrderBy(h => h.StartDate)
            .ToListAsync(cancellationToken);

        return results.AsReadOnly();
    }

    /// <summary>
    /// Retrieves cost history records for a specific product.
    /// </summary>
    public async Task<IReadOnlyList<ProductCostHistory>> GetCostHistoryByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var results = await DbContext.ProductCostHistories
            .AsNoTracking()
            .Where(h => h.ProductId == productId)
            .OrderBy(h => h.StartDate)
            .ToListAsync(cancellationToken);

        return results.AsReadOnly();
    }

    #region Private Methods

    /// <summary>
    /// Builds the base product list query with standard includes for list and search operations.
    /// </summary>
    private IQueryable<Product> BuildProductListQuery()
    {
        return DbContext.Products
            .AsNoTracking()
            .Include(p => p.ProductSubcategory)
                .ThenInclude(sc => sc.ProductCategory)
            .Include(p => p.ProductModel)
            .AsQueryable();
    }

    private static string EscapeLikePattern(string value) => LikePatternHelper.EscapeLikePattern(value);

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductParameter parameters)
    {
        return parameters.OrderBy switch
        {
            SortedResultConstants.ProductId => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(p => p.ProductId)
                : query.OrderByDescending(p => p.ProductId),
            SortedResultConstants.Name => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),
            SortedResultConstants.ProductNumber => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(p => p.ProductNumber)
                : query.OrderByDescending(p => p.ProductNumber),
            SortedResultConstants.ListPrice => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(p => p.ListPrice)
                : query.OrderByDescending(p => p.ListPrice),
            SortedResultConstants.StandardCost => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(p => p.StandardCost)
                : query.OrderByDescending(p => p.StandardCost),
            _ => query.OrderBy(p => p.ProductId)
        };
    }

    #endregion Private Methods
}
