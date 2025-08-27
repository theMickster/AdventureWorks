namespace AdventureWorks.Models.Features.Sales;

/// <summary>
/// Read model for the survey demographics that AdventureWorks tracks per store.
/// Survey fields are nullable because <c>Sales.Store.Demographics</c> may be NULL,
/// malformed, or contain only a subset of the published <c>StoreSurvey</c> elements.
/// </summary>
public sealed class StoreDemographicsModel
{
    /// <summary>The store's BusinessEntityId.</summary>
    public int StoreId { get; set; }

    /// <summary>The store's display name.</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>Annual sales reported by the store, in USD.</summary>
    public decimal? AnnualSales { get; set; }

    /// <summary>Annual revenue reported by the store, in USD.</summary>
    public decimal? AnnualRevenue { get; set; }

    /// <summary>Primary banking partner.</summary>
    public string? BankName { get; set; }

    /// <summary>Business classification code (BM, BS, D, OS, SGS).</summary>
    public string? BusinessType { get; set; }

    /// <summary>Year the store opened.</summary>
    public int? YearOpened { get; set; }

    /// <summary>Specialty focus (Family, Kids, BMX, Touring, Road, Mountain, All).</summary>
    public string? Specialty { get; set; }

    /// <summary>Floor space in square feet.</summary>
    public int? SquareFeet { get; set; }

    /// <summary>
    /// Internet connectivity descriptor (e.g. <c>56kb</c>, <c>ISDN</c>, <c>DSL</c>,
    /// <c>T1</c>, <c>T2</c>, <c>T3</c>) per the StoreSurvey XSD enumeration.
    /// </summary>
    public string? Internet { get; set; }

    /// <summary>Number of employees.</summary>
    public int? NumberEmployees { get; set; }

    /// <summary>Number of bicycle brands carried (AW, 2, 3, 4+).</summary>
    public string? Brands { get; set; }
}
