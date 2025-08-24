using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides data access operations for product reviews.
/// </summary>
[ServiceLifetimeScoped]
public sealed class ProductReviewRepository(AdventureWorksDbContext dbContext)
    : EfRepository<ProductReview>(dbContext), IProductReviewRepository
{
    /// <summary>
    /// Retrieves a paginated list of product reviews for a given product and the total count of matching reviews.
    /// </summary>
    /// <param name="productId">the unique product identifier</param>
    /// <param name="parameters">the input paging parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<(IReadOnlyList<ProductReview>, int)> GetProductReviewsByProductIdAsync(
        int productId,
        ProductReviewParameter parameters,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.ProductReviews
            .AsNoTracking()
            .Where(x => x.ProductId == productId);

        var totalCount = await query.CountAsync(cancellationToken);

        query = parameters.OrderBy switch
        {
            SortedResultConstants.ProductReviewId => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.ProductReviewId)
                : query.OrderByDescending(x => x.ProductReviewId),
            SortedResultConstants.Rating => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.Rating)
                : query.OrderByDescending(x => x.Rating),
            SortedResultConstants.ReviewDate => parameters.SortOrder == SortedResultConstants.Ascending
                ? query.OrderBy(x => x.ReviewDate)
                : query.OrderByDescending(x => x.ReviewDate),
            _ => query.OrderBy(x => x.ProductReviewId)
        };

        query = query.Skip(parameters.GetRecordsToSkip()).Take(parameters.PageSize);

        var results = await query.ToListAsync(cancellationToken);

        return (results.AsReadOnly(), totalCount);
    }

    /// <summary>
    /// Retrieves rating distribution for a given product as a SQL GROUP BY aggregate.
    /// </summary>
    /// <param name="productId">the unique product identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<IReadOnlyDictionary<int, int>> GetRatingDistributionByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var raw = await DbContext.ProductReviews
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .GroupBy(x => x.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, cancellationToken);

        return raw;
    }
}
