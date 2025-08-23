using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories.Production;

public interface IProductRepository : IAsyncRepository<Product>
{
    /// <summary>
    /// Retrieves a product by its unique identifier with all related entities loaded
    /// (category, subcategory, model, photos, inventory).
    /// </summary>
    /// <param name="productId">The unique product identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The product entity with navigation properties included, or <c>null</c> if not found.</returns>
    Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of products and the total count of products in the database.
    /// </summary>
    /// <param name="parameters">Paging and sort parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A tuple of the product page and the total unfiltered count.</returns>
    Task<(IReadOnlyList<Product>, int)> GetProductsAsync(ProductParameter parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a filtered, paginated list of products and the total count matching the filter.
    /// </summary>
    /// <param name="parameters">Paging and sort parameters.</param>
    /// <param name="searchModel">Filter criteria (name, category, price range, etc.).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A tuple of the matching product page and the total filtered count.</returns>
    Task<(IReadOnlyList<Product>, int)> SearchProductsAsync(ProductParameter parameters, ProductSearchModel searchModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all product categories.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All product categories in the database (4 in AdventureWorks).</returns>
    Task<IReadOnlyList<ProductCategory>> GetProductCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves product subcategories, optionally filtered by parent category.
    /// </summary>
    /// <param name="categoryId">When provided, returns only subcategories belonging to this category.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Subcategory records, filtered by <paramref name="categoryId"/> when supplied.</returns>
    Task<IReadOnlyList<ProductSubcategory>> GetProductSubcategoriesAsync(int? categoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves inventory records (stock by warehouse location) for a specific product.
    /// </summary>
    /// <param name="productId">The unique product identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All inventory location rows for the product.</returns>
    Task<IReadOnlyList<ProductInventory>> GetProductInventoryByProductIdAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves list price history records for a specific product.
    /// </summary>
    /// <param name="productId">The unique product identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All list price history rows for the product, ordered by start date.</returns>
    Task<IReadOnlyList<ProductListPriceHistory>> GetListPriceHistoryByProductIdAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves standard cost history records for a specific product.
    /// </summary>
    /// <param name="productId">The unique product identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All cost history rows for the product, ordered by start date.</returns>
    Task<IReadOnlyList<ProductCostHistory>> GetCostHistoryByProductIdAsync(int productId, CancellationToken cancellationToken = default);
}
