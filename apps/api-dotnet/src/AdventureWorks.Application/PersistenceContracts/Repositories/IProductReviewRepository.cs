using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

/// <summary>
/// Defines the contract for product review data access operations.
/// </summary>
public interface IProductReviewRepository : IAsyncRepository<ProductReview>
{
    /// <summary>
    /// Retrieves a paginated list of product reviews for a given product and the total count of matching reviews.
    /// </summary>
    /// <param name="productId">the unique product identifier</param>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task<(IReadOnlyList<ProductReview>, int)> GetProductReviewsByProductIdAsync(int productId, ProductReviewParameter parameters, CancellationToken cancellationToken = default);
}
