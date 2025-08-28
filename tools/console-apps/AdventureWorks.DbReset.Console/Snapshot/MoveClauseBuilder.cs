using System.Globalization;
using System.Text;

namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>One <c>(name, value)</c> pair for a <c>RESTORE … MOVE</c> clause parameter.</summary>
internal readonly record struct MoveClauseParameter(string Name, string Value);

/// <summary>SQL fragment plus the <see cref="SqlParameter"/> bindings produced by <see cref="MoveClauseBuilder.Build"/>.</summary>
internal readonly record struct MoveClauseResult(string SqlFragment, IReadOnlyList<MoveClauseParameter> Parameters);

/// <summary>
/// Pure function: <c>(BackupFileMetadata[], targetDb, TargetFileSystem, ticks)</c> →
/// <c>(MOVE-clause SQL, parameter list)</c>. No I/O, no time, no SQL execution.
/// </summary>
/// <remarks>
/// <b>Why ticks for FILESTREAM:</b> SQL Server requires the FILESTREAM container directory to
/// <i>not</i> exist when <c>RESTORE</c> runs. Repeated restores into the same target database
/// would collide on the previous container path and fail with error <c>5121</c>
/// ("directory already exists"). Suffixing the per-restore <see cref="DateTime.Ticks"/> makes
/// each <c>{targetDb}_FS{n}_{ticks}</c> path unique without orchestrator-side cleanup of stale
/// FILESTREAM directories. Data and log files don't need this — <c>WITH REPLACE</c> permits
/// overwriting <c>.mdf</c>/<c>.ndf</c>/<c>.ldf</c> in place. ADO Story #925.
/// </remarks>
internal static class MoveClauseBuilder
{
    /// <summary>
    /// Walks <paramref name="files"/> in order, mapping each row to a <c>MOVE @logicalN TO @physicalN</c>
    /// pair. Per-type counters drive the physical filename:
    /// <list type="bullet">
    ///   <item><description><c>'D'</c>: first → <c>{targetDb}.mdf</c>, subsequent → <c>{targetDb}_{n}.ndf</c> in <see cref="TargetFileSystem.DataDir"/></description></item>
    ///   <item><description><c>'L'</c>: first → <c>{targetDb}_log.ldf</c>, subsequent → <c>{targetDb}_log_{n}.ldf</c> in <see cref="TargetFileSystem.LogDir"/></description></item>
    ///   <item><description><c>'S'</c>: <c>{targetDb}_FS{n}_{ticks}</c> in <see cref="TargetFileSystem.DataDir"/></description></item>
    /// </list>
    /// </summary>
    /// <param name="ticksForFilestreamSuffix">Injected for determinism in tests; production passes <see cref="DateTime.UtcNow"/>.<see cref="DateTime.Ticks"/>.</param>
    /// <exception cref="NotSupportedException">A row carries an unrecognized <c>Type</c>.</exception>
    public static MoveClauseResult Build(
        IReadOnlyList<BackupFileMetadata> files,
        string targetDb,
        TargetFileSystem fs,
        long ticksForFilestreamSuffix)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDb);
        ArgumentNullException.ThrowIfNull(fs);

        if (files.Count == 0)
        {
            return new MoveClauseResult(string.Empty, Array.Empty<MoveClauseParameter>());
        }

        var parameters = new List<MoveClauseParameter>(files.Count * 2);
        var sql = new StringBuilder();

        var dataIndex = 0;
        var logIndex = 0;
        var fsIndex = 0;

        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];
            string physical;
            switch (file.Type)
            {
                case 'D':
                    physical = fs.CombineData(
                        dataIndex == 0
                            ? string.Concat(targetDb, ".mdf")
                            : string.Concat(targetDb, "_", dataIndex.ToString(CultureInfo.InvariantCulture), ".ndf"));
                    dataIndex++;
                    break;
                case 'L':
                    physical = fs.CombineLog(
                        logIndex == 0
                            ? string.Concat(targetDb, "_log.ldf")
                            : string.Concat(targetDb, "_log_", logIndex.ToString(CultureInfo.InvariantCulture), ".ldf"));
                    logIndex++;
                    break;
                case 'S':
                    physical = fs.CombineData(
                        string.Concat(
                            targetDb,
                            "_FS",
                            fsIndex.ToString(CultureInfo.InvariantCulture),
                            "_",
                            ticksForFilestreamSuffix.ToString(CultureInfo.InvariantCulture)));
                    fsIndex++;
                    break;
                default:
                    throw new NotSupportedException(
                        $"Unsupported FILELISTONLY Type '{file.Type}' for logical file '{file.LogicalName}'.");
            }

            var logicalParam = string.Concat("@logical", i.ToString(CultureInfo.InvariantCulture));
            var physicalParam = string.Concat("@physical", i.ToString(CultureInfo.InvariantCulture));

            if (i > 0)
            {
                sql.Append(",\n    ");
            }
            else
            {
                sql.Append("    ");
            }
            sql.Append("MOVE ").Append(logicalParam).Append(" TO ").Append(physicalParam);

            parameters.Add(new MoveClauseParameter(logicalParam, file.LogicalName));
            parameters.Add(new MoveClauseParameter(physicalParam, physical));
        }

        return new MoveClauseResult(sql.ToString(), parameters);
    }
}
