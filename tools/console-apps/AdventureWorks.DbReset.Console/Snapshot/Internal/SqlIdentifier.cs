using System.Text.RegularExpressions;

namespace AdventureWorks.DbReset.Console.Snapshot.Internal;

/// <summary>
/// Two-layer defense for SQL identifier interpolation.
/// <list type="bullet">
///   <item><description><see cref="ValidateAgainstPattern"/> — config-defined allowlist (e.g. opt-in test database names). Runs first.</description></item>
///   <item><description><see cref="Quote"/> — universal bracket-escaping. Runs last, just before string interpolation into SQL.</description></item>
/// </list>
/// </summary>
internal static class SqlIdentifier
{
    /// <summary>
    /// Wraps <paramref name="name"/> in <c>[ ]</c> after escaping any <c>]</c> by doubling.
    /// Rejects empty/whitespace, sysname-overlong (>128 chars), and control characters
    /// (<c>\0</c>, <c>\r</c>, <c>\n</c>).
    /// </summary>
    /// <exception cref="ArgumentException">The identifier is null/empty/whitespace, exceeds sysname, or contains forbidden characters.</exception>
    public static string Quote(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 128)
        {
            throw new ArgumentException("Identifier exceeds sysname (128 chars).", nameof(name));
        }
        if (name.IndexOfAny(['\0', '\r', '\n']) >= 0)
        {
            throw new ArgumentException("Identifier contains forbidden characters.", nameof(name));
        }
        return $"[{name.Replace("]", "]]")}]";
    }

    /// <summary>
    /// Throws if <paramref name="name"/> does not match <paramref name="pattern"/>. Does not
    /// modify the input.
    /// </summary>
    /// <exception cref="ArgumentNullException">Either argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">The identifier does not match the pattern.</exception>
    public static void ValidateAgainstPattern(string name, Regex pattern)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(pattern);
        if (!pattern.IsMatch(name))
        {
            throw new ArgumentException(
                $"Identifier '{name}' does not match required pattern '{pattern}'.",
                nameof(name));
        }
    }
}
