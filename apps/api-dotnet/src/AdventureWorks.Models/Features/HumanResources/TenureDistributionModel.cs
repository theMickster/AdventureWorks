namespace AdventureWorks.Models.Features.HumanResources;

public sealed class TenureDistributionModel
{
    /// <summary>Active employees with less than 1 year of tenure.</summary>
    public int UnderOneYear { get; set; }

    /// <summary>Active employees with 1–3 years of tenure.</summary>
    public int OneToThreeYears { get; set; }

    /// <summary>Active employees with 3–5 years of tenure.</summary>
    public int ThreeToFiveYears { get; set; }

    /// <summary>Active employees with 5–10 years of tenure.</summary>
    public int FiveToTenYears { get; set; }

    /// <summary>Active employees with 10 or more years of tenure.</summary>
    public int TenPlusYears { get; set; }
}
