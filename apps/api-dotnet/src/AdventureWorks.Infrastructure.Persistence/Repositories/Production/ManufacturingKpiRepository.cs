using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Models.Features.Production;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Production;

/// <summary>
/// EF Core implementation of <see cref="IManufacturingKpiRepository"/>.
/// </summary>
[ServiceLifetimeScoped]
public sealed class ManufacturingKpiRepository(AdventureWorksDbContext dbContext) : IManufacturingKpiRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// Aggregates work-order quantities and derives the overall manufacturing percentages.
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The aggregate manufacturing KPI model.</returns>
    public async Task<ManufacturingKpisModel> GetManufacturingKpisAsync(CancellationToken cancellationToken = default)
    {
        var totalWorkOrders = await _dbContext.WorkOrders
            .AsNoTracking()
            .CountAsync(cancellationToken);
        var totalOrdered = await _dbContext.WorkOrders
            .AsNoTracking()
            .SumAsync(workOrder => workOrder.OrderQty, cancellationToken);
        var totalScrapped = await _dbContext.WorkOrders
            .AsNoTracking()
            .SumAsync(workOrder => workOrder.ScrappedQty, cancellationToken);
        var totalStocked = totalOrdered - totalScrapped;

        var overallYieldPct = totalOrdered == 0 ? 0m : Math.Round(totalStocked / (decimal)totalOrdered * 100m, 2);
        var overallScrapPct = totalOrdered == 0 ? 0m : Math.Round(totalScrapped / (decimal)totalOrdered * 100m, 2);

        return new ManufacturingKpisModel
        {
            TotalWorkOrders = totalWorkOrders,
            TotalOrdered = totalOrdered,
            TotalStocked = totalStocked,
            TotalScrapped = totalScrapped,
            OverallYieldPct = overallYieldPct,
            OverallScrapPct = overallScrapPct
        };
    }
}
