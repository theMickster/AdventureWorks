namespace AdventureWorks.Common.Helpers;

/// <summary>
/// Utility methods for building safe SQL Server LIKE patterns.
/// </summary>
public static class LikePatternHelper
{
    /// <summary>
    /// Escapes SQL Server LIKE wildcard characters in a user-supplied value so that
    /// the value is treated as a literal string rather than a pattern.
    /// Escapes: <c>[</c> → <c>[[]</c>, <c>%</c> → <c>[%]</c>, <c>_</c> → <c>[_]</c>.
    /// </summary>
    /// <param name="value">the raw search term to escape</param>
    public static string EscapeLikePattern(string value)
    {
        return value
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }
}
