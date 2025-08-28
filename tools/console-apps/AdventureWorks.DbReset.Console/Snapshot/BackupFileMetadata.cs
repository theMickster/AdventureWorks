namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// One row of <c>RESTORE FILELISTONLY</c>. Internal — visible to the test assembly via
/// <c>InternalsVisibleTo</c> so tests can hand-construct rows.
/// </summary>
/// <param name="LogicalName">Logical file name (becomes <c>MOVE @logical_n TO @physical_n</c>). Untrusted; passed via <c>SqlParameter</c>.</param>
/// <param name="PhysicalName">Original physical file name from the backup. Informational only.</param>
/// <param name="Type"><c>'D'</c> data, <c>'L'</c> log, <c>'S'</c> FILESTREAM. Unknown values throw downstream.</param>
/// <param name="Size">Size in bytes; summed for <see cref="BaselineStatus.SizeBytes"/>.</param>
internal sealed record BackupFileMetadata(
    string LogicalName,
    string PhysicalName,
    char Type,
    long Size);
