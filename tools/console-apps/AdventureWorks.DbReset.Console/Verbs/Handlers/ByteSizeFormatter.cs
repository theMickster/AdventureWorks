using System.Globalization;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Formats byte counts as binary-prefix strings (B, KiB, MiB, GiB, TiB) with a single decimal
/// place above the kilobyte boundary. Centralized so verb output is consistent regardless of
/// whether the size came from <c>RESTORE FILELISTONLY</c> (logical size) or <see cref="System.IO.FileInfo"/>
/// (on-disk size). Uses <see cref="CultureInfo.InvariantCulture"/> so output is stable across
/// locales — operators paste these strings into bug reports verbatim.
/// </summary>
internal static class ByteSizeFormatter
{
    private const long KiB = 1024L;
    private const long MiB = KiB * 1024L;
    private const long GiB = MiB * 1024L;
    private const long TiB = GiB * 1024L;

    /// <summary>
    /// Formats <paramref name="bytes"/> using the smallest binary unit that keeps the numeric
    /// value below 1024. Below 1 KiB the value is rendered as an integer count of bytes
    /// (e.g. <c>1023 B</c>, never <c>1023.0 B</c>) because fractional bytes are nonsensical.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bytes"/> is negative.</exception>
    public static string Format(long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), bytes, "Byte count cannot be negative.");
        }

        if (bytes < KiB)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} B", bytes);
        }
        if (bytes < MiB)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} KiB", (double)bytes / KiB);
        }
        if (bytes < GiB)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} MiB", (double)bytes / MiB);
        }
        if (bytes < TiB)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0} GiB", (double)bytes / GiB);
        }
        return string.Format(CultureInfo.InvariantCulture, "{0:0.0} TiB", (double)bytes / TiB);
    }
}
