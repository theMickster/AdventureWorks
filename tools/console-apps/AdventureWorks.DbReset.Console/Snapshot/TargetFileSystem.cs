namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// Default data/log directories of a SQL Server instance plus the path separator detected from
/// them. Constructed by the orchestrator's <c>ReadTargetFileSystemAsync</c>; never built from
/// constants.
/// </summary>
internal sealed record TargetFileSystem(string DataDir, string LogDir, char Separator)
{
    /// <summary>
    /// Detects the path separator from a sample SQL-Server-returned path. UNC paths
    /// (<c>\\server\share\…</c>) and Windows local paths both yield <c>'\\'</c>;
    /// anything else yields <c>'/'</c>.
    /// </summary>
    public static char DetectSeparator(string samplePath)
    {
        ArgumentNullException.ThrowIfNull(samplePath);
        return samplePath.Contains('\\') ? '\\' : '/';
    }

    /// <summary>Combines <see cref="DataDir"/> with <paramref name="filename"/> using <see cref="Separator"/>.</summary>
    public string CombineData(string filename) => Combine(DataDir, filename);

    /// <summary>Combines <see cref="LogDir"/> with <paramref name="filename"/> using <see cref="Separator"/>.</summary>
    public string CombineLog(string filename) => Combine(LogDir, filename);

    private string Combine(string dir, string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);
        if (string.IsNullOrEmpty(dir))
        {
            return filename;
        }
        return dir[^1] == Separator
            ? string.Concat(dir, filename)
            : string.Concat(dir, Separator.ToString(), filename);
    }
}
