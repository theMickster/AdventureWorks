using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Default <see cref="IVerifyBaselineHandler"/>. Asks the snapshot provider whether the
/// baseline file is reachable, then composes a single operator-facing line for stdout (Present)
/// or stderr (Missing).
/// </summary>
/// <remarks>
/// <para>
/// <b>Two size sources, two messages:</b> the snapshot provider always returns the
/// <c>RESTORE FILELISTONLY</c> logical size (sum of data + log + FILESTREAM rows). When the
/// .bak file lives on a path the tool process can stat (typical local-dev or developer-machine
/// scenario), we ALSO read the on-disk file size — that's the number operators want to see for
/// disk-budget decisions. When the .bak is only visible to SQL Server (e.g. inside a Docker
/// container's filesystem, or on a UNC path the dev machine can't reach), we fall back to the
/// logical size and explicitly say so to avoid an operator wondering "where's the file size?".
/// </para>
/// </remarks>
internal sealed class VerifyBaselineHandler : IVerifyBaselineHandler
{
    private readonly IDatabaseSnapshotProvider _provider;
    private readonly DbResetOptions _options;

    public VerifyBaselineHandler(IDatabaseSnapshotProvider provider, DbResetOptions options)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(options);

        _provider = provider;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<VerbResult> RunAsync(string targetName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);

        var status = await _provider.BaselineExistsAsync(targetName, ct);

        if (!status.Exists)
        {
            // Single-line stderr: name the verb the operator should run next.
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                DbResetDefaults.BaselineMissingGuidanceFormat,
                status.Path);
            return VerbResult.Fail(DbResetDefaults.ExitVerifyBaselineMissing, msg);
        }

        var logicalSize = status.SizeBytes ?? 0L;
        string message;
        if (File.Exists(_options.BaselinePath))
        {
            var onDisk = new FileInfo(_options.BaselinePath).Length;
            message = string.Format(
                CultureInfo.InvariantCulture,
                "Baseline at {0}: {1} on disk",
                status.Path,
                ByteSizeFormatter.Format(onDisk));
        }
        else
        {
            message = string.Format(
                CultureInfo.InvariantCulture,
                "Baseline at {0}: {1} logical (FILELISTONLY total; .bak file not visible from this process)",
                status.Path,
                ByteSizeFormatter.Format(logicalSize));
        }

        return VerbResult.Ok(message);
    }
}
