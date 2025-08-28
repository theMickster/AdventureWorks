namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// Result of <see cref="IDatabaseSnapshotProvider.BaselineExistsAsync"/>. The probed
/// <see cref="Path"/> is always populated so the verb's diagnostic output names the exact file
/// that was checked, even when the file is missing. <see cref="SizeBytes"/> is the sum of
/// <c>RESTORE FILELISTONLY</c> row sizes (data + log + FILESTREAM), not the on-disk
/// <c>.bak</c> size.
/// </summary>
public sealed record BaselineStatus(bool Exists, long? SizeBytes, string Path)
{
    /// <summary>Constructs a <c>Missing</c> status — the baseline file could not be read.</summary>
    public static BaselineStatus Missing(string path) => new(false, null, path);

    /// <summary>Constructs a <c>Present</c> status with the summed FILELISTONLY size.</summary>
    public static BaselineStatus Present(string path, long sizeBytes) => new(true, sizeBytes, path);
}
