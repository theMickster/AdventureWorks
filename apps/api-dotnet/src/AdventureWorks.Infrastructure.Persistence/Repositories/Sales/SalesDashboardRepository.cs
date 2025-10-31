using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Models.Features.Sales;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

/// <summary>
/// EF Core implementation of <see cref="ISalesDashboardRepository"/> that executes aggregate
/// queries against the AdventureWorks database and assembles the dashboard response model.
/// </summary>
[ServiceLifetimeScoped]
public sealed class SalesDashboardRepository(AdventureWorksDbContext dbContext)
    : ISalesDashboardRepository
{
    private readonly AdventureWorksDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    /// <inheritdoc/>
    public async Task<SalesDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        // Query 1: Overall KPIs — single query for snapshot consistency
        var kpis = await _dbContext.SalesOrderHeaders
            .AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new { TotalRevenue = g.Sum(x => x.TotalDue), OrderCount = g.Count() })
            .FirstOrDefaultAsync(cancellationToken);

        var totalRevenue = kpis?.TotalRevenue ?? 0m;
        var orderCount = kpis?.OrderCount ?? 0;
        var avgOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0m;

        // Query 2: Top performer aggregates by SalesPersonId
        var topAggregates = await _dbContext.SalesOrderHeaders
            .AsNoTracking()
            .Where(o => o.SalesPersonId != null)
            .GroupBy(o => o.SalesPersonId)
            .Select(g => new { SalesPersonId = g.Key!.Value, Revenue = g.Sum(x => x.TotalDue), OrderCount = g.Count() })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Query 3: Person names for top 5 (separate query — safer EF Core GroupBy behavior)
        var topIds = topAggregates.Select(x => x.SalesPersonId).ToList();
        var personNames = await _dbContext.SalesPersons
            .AsNoTracking()
            .Where(sp => topIds.Contains(sp.BusinessEntityId))
            .Select(sp => new
            {
                sp.BusinessEntityId,
                sp.Employee.PersonBusinessEntity.FirstName,
                sp.Employee.PersonBusinessEntity.LastName
            })
            .ToListAsync(cancellationToken);

        var topPerformers = topAggregates.Select(agg =>
        {
            var person = personNames.FirstOrDefault(p => p.BusinessEntityId == agg.SalesPersonId);
            return new DashboardTopPerformerModel
            {
                SalesPersonId = agg.SalesPersonId,
                Name = person is not null ? $"{person.FirstName} {person.LastName}" : string.Empty,
                Revenue = agg.Revenue,
                OrderCount = agg.OrderCount
            };
        }).ToList();

        // Query 4: Territory aggregates by TerritoryId
        var territoryAggregates = await _dbContext.SalesOrderHeaders
            .AsNoTracking()
            .Where(o => o.TerritoryId != null)
            .GroupBy(o => o.TerritoryId)
            .Select(g => new { TerritoryId = g.Key!.Value, Revenue = g.Sum(x => x.TotalDue), OrderCount = g.Count() })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync(cancellationToken);

        // Query 5: Territory names (separate query)
        var territoryIds = territoryAggregates.Select(x => x.TerritoryId).ToList();
        var territories = await _dbContext.SalesTerritories
            .AsNoTracking()
            .Where(t => territoryIds.Contains(t.TerritoryId))
            .Select(t => new { t.TerritoryId, t.Name, t.CountryRegionCode })
            .ToListAsync(cancellationToken);

        var territoryBreakdown = territoryAggregates.Select(agg =>
        {
            var territory = territories.FirstOrDefault(t => t.TerritoryId == agg.TerritoryId);
            return new DashboardTerritoryModel
            {
                TerritoryId = agg.TerritoryId,
                Name = territory?.Name ?? string.Empty,
                CountryCode = territory?.CountryRegionCode ?? string.Empty,
                OrderCount = agg.OrderCount,
                Revenue = agg.Revenue
            };
        }).ToList();

        // Query 6: Max date for trend anchor (dataset is historical, max date is 2014-06-30)
        // Use nullable projection so MaxAsync returns null on an empty table rather than throwing
        var maxDate = await _dbContext.SalesOrderHeaders
            .AsNoTracking()
            .Select(x => (DateTime?)x.OrderDate)
            .MaxAsync(cancellationToken);

        List<DashboardMonthlySalesTrendModel> monthlySalesTrend;

        if (maxDate is null)
        {
            monthlySalesTrend = [];
        }
        else
        {
            var cutoff = maxDate.Value.AddMonths(-23); // 24 months inclusive

            // Query 7: Monthly trend
            monthlySalesTrend = await _dbContext.SalesOrderHeaders
                .AsNoTracking()
                .Where(o => o.OrderDate >= cutoff)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new DashboardMonthlySalesTrendModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(x => x.TotalDue)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);
        }

        return new SalesDashboardModel
        {
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            AverageOrderValue = avgOrderValue,
            TopPerformers = topPerformers,
            TerritoryBreakdown = territoryBreakdown,
            MonthlySalesTrend = monthlySalesTrend
        };
    }
}
