using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Models.Features.Production;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Production;

/// <summary>
/// EF Core implementation of <see cref="IManufacturingQualityScorecardRepository"/>.
/// </summary>
[ServiceLifetimeScoped]
public sealed class ManufacturingQualityScorecardRepository(AdventureWorksDbContext dbContext)
    : IManufacturingQualityScorecardRepository
{
    private const int RankingLimit = 5;

    private readonly AdventureWorksDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// Aggregates work-order quantities into product rankings and a scrap-reason breakdown.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The manufacturing quality scorecard.</returns>
    public async Task<ManufacturingQualityScorecardModel> GetQualityScorecardAsync(
        CancellationToken cancellationToken = default)
    {
        var topScrapped = await _dbContext.WorkOrders
            .AsNoTracking()
            .Where(workOrder => workOrder.ScrappedQty > 0)
            .GroupBy(workOrder => workOrder.ProductId)
            .Select(group => new ProductAggregate
            {
                ProductId = group.Key,
                OrderedQty = group.Sum(workOrder => workOrder.OrderQty),
                StockedQty = group.Sum(workOrder => workOrder.StockedQty),
                ScrappedQty = group.Sum(workOrder => workOrder.ScrappedQty)
            })
            .OrderByDescending(product => product.ScrappedQty)
            .ThenBy(product => product.ProductId)
            .Take(RankingLimit)
            .ToListAsync(cancellationToken);

        var bottomYield = await _dbContext.WorkOrders
            .AsNoTracking()
            .Where(workOrder => workOrder.OrderQty > 0)
            .GroupBy(workOrder => workOrder.ProductId)
            .Select(group => new ProductAggregate
            {
                ProductId = group.Key,
                OrderedQty = group.Sum(workOrder => workOrder.OrderQty),
                StockedQty = group.Sum(workOrder => workOrder.StockedQty),
                ScrappedQty = group.Sum(workOrder => workOrder.ScrappedQty)
            })
            .ToListAsync(cancellationToken);

        var bottomYieldRanked = bottomYield
            .OrderBy(product => CalculatePercentage(product.StockedQty, product.OrderedQty))
            .ThenBy(product => product.ProductId)
            .Take(RankingLimit)
            .ToList();

        var scrapReasonAggregates = await _dbContext.WorkOrders
            .AsNoTracking()
            .Where(workOrder => workOrder.ScrappedQty > 0 && workOrder.ScrapReasonId.HasValue)
            .GroupBy(workOrder => workOrder.ScrapReasonId!.Value)
            .Select(group => new ScrapReasonAggregate
            {
                ScrapReasonId = group.Key,
                ScrappedQty = group.Sum(workOrder => workOrder.ScrappedQty)
            })
            .OrderByDescending(reason => reason.ScrappedQty)
            .ThenBy(reason => reason.ScrapReasonId)
            .ToListAsync(cancellationToken);

        var productIds = topScrapped
            .Concat(bottomYieldRanked)
            .Select(product => product.ProductId)
            .Distinct()
            .ToArray();
        var productNames = await GetProductNamesAsync(productIds, cancellationToken);

        var scrapReasonIds = scrapReasonAggregates
            .Select(reason => reason.ScrapReasonId)
            .ToArray();
        var scrapReasonNames = await GetScrapReasonNamesAsync(scrapReasonIds, cancellationToken);
        var categorizedScrapQty = scrapReasonAggregates.Sum(reason => reason.ScrappedQty);

        return new ManufacturingQualityScorecardModel
        {
            Top5ByScrapped = topScrapped
                .Select(product => MapProduct(product, productNames))
                .ToList(),
            Bottom5ByYield = bottomYieldRanked
                .Select(product => MapProduct(product, productNames))
                .ToList(),
            ScrapReasonBreakdown = scrapReasonAggregates
                .Select(reason => new QualityScorecardScrapReasonModel
                {
                    ScrapReasonId = reason.ScrapReasonId,
                    ScrapReasonName = scrapReasonNames.GetValueOrDefault(reason.ScrapReasonId, string.Empty),
                    ScrappedQty = reason.ScrappedQty,
                    ScrapPct = CalculatePercentage(reason.ScrappedQty, categorizedScrapQty)
                })
                .ToList()
        };
    }

    private async Task<Dictionary<int, string>> GetProductNamesAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => productIds.Contains(product.ProductId))
            .ToDictionaryAsync(product => product.ProductId, product => product.Name, cancellationToken);
    }

    private async Task<Dictionary<short, string>> GetScrapReasonNamesAsync(
        IReadOnlyCollection<short> scrapReasonIds,
        CancellationToken cancellationToken)
    {
        if (scrapReasonIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.ScrapReasons
            .AsNoTracking()
            .Where(reason => scrapReasonIds.Contains(reason.ScrapReasonId))
            .ToDictionaryAsync(reason => reason.ScrapReasonId, reason => reason.Name, cancellationToken);
    }

    private static QualityScorecardProductModel MapProduct(
        ProductAggregate aggregate,
        IReadOnlyDictionary<int, string> productNames)
    {
        return new QualityScorecardProductModel
        {
            ProductId = aggregate.ProductId,
            ProductName = productNames.GetValueOrDefault(aggregate.ProductId, string.Empty),
            OrderedQty = aggregate.OrderedQty,
            StockedQty = aggregate.StockedQty,
            ScrappedQty = aggregate.ScrappedQty,
            YieldPct = CalculatePercentage(aggregate.StockedQty, aggregate.OrderedQty),
            ScrapPct = CalculatePercentage(aggregate.ScrappedQty, aggregate.OrderedQty)
        };
    }

    private static decimal CalculatePercentage(int numerator, int denominator)
    {
        return denominator == 0
            ? 0m
            : Math.Round(numerator / (decimal)denominator * 100m, 2);
    }

    private sealed class ProductAggregate
    {
        public int ProductId { get; init; }

        public int OrderedQty { get; init; }

        public int StockedQty { get; init; }

        public int ScrappedQty { get; init; }
    }

    private sealed class ScrapReasonAggregate
    {
        public short ScrapReasonId { get; init; }

        public int ScrappedQty { get; init; }
    }
}
