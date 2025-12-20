namespace AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;

/// <summary>
/// Maps the <c>Purchasing.Vendor.CreditRating</c> byte code to its human-readable label.
/// </summary>
internal static class CreditRatingLabels
{
    /// <summary>
    /// Returns the human-readable label for a vendor credit rating code.
    /// </summary>
    /// <param name="creditRating">1=Superior, 2=Excellent, 3=Above Average, 4=Average, 5=Below Average.</param>
    /// <returns>The matching label, or "Unknown" for any unrecognised code.</returns>
    public static string GetLabel(byte creditRating) => creditRating switch
    {
        1 => "Superior",
        2 => "Excellent",
        3 => "Above Average",
        4 => "Average",
        5 => "Below Average",
        _ => "Unknown"
    };
}
