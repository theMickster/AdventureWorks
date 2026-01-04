namespace AdventureWorks.Models.Features.Production;

/// <summary>
/// Ranked manufacturing quality data for the production dashboard.
/// </summary>
public sealed class ManufacturingQualityScorecardModel
{
    /// <summary>Products with the highest total scrapped quantity, limited to five.</summary>
    public List<QualityScorecardProductModel> Top5ByScrapped { get; set; } = [];

    /// <summary>Products with the lowest aggregate yield, limited to five.</summary>
    public List<QualityScorecardProductModel> Bottom5ByYield { get; set; } = [];

    /// <summary>Scrap quantities grouped by the recorded scrap reason.</summary>
    public List<QualityScorecardScrapReasonModel> ScrapReasonBreakdown { get; set; } = [];
}

/// <summary>
/// Aggregate quality metrics for one product in a scorecard ranking.
/// </summary>
public sealed class QualityScorecardProductModel
{
    /// <summary>The identifier of the manufactured product.</summary>
    public int ProductId { get; set; }

    /// <summary>The display name of the manufactured product.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>The aggregate quantity ordered for the product.</summary>
    public int OrderedQty { get; set; }

    /// <summary>The aggregate quantity stocked for the product.</summary>
    public int StockedQty { get; set; }

    /// <summary>The aggregate quantity scrapped for the product.</summary>
    public int ScrappedQty { get; set; }

    /// <summary>The stocked quantity as a percentage of ordered quantity.</summary>
    public decimal YieldPct { get; set; }

    /// <summary>The scrapped quantity as a percentage of ordered quantity.</summary>
    public decimal ScrapPct { get; set; }
}

/// <summary>
/// Aggregate scrap metrics for one recorded scrap reason.
/// </summary>
public sealed class QualityScorecardScrapReasonModel
{
    /// <summary>The identifier of the scrap reason.</summary>
    public short ScrapReasonId { get; set; }

    /// <summary>The display name of the scrap reason.</summary>
    public string ScrapReasonName { get; set; } = string.Empty;

    /// <summary>The aggregate quantity scrapped for this reason.</summary>
    public int ScrappedQty { get; set; }

    /// <summary>This reason's share of scrap with a recorded reason.</summary>
    public decimal ScrapPct { get; set; }
}
