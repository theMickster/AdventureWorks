namespace AdventureWorks.API.libs.InternalHelpers;

/// <summary>
/// Strips control characters from values before they are written to structured log tokens,
/// preventing forged log entries (CWE-117) from user-controlled input such as headers or request bodies.
/// </summary>
internal static class LogSanitizer
{
    internal static string? Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return string.Create(value.Length, value, static (span, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                span[i] = char.IsControl(c) ? '_' : c;
            }
        });
    }
}
