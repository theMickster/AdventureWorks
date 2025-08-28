using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Default <see cref="ISnapshotHandler"/>. Drives
/// <see cref="IDatabaseSnapshotProvider.CreateSnapshotAsync"/>, times the wall-clock cost, and
/// composes the operator-facing summary. Known failure modes (source unreachable / OS error 5)
/// are mapped to typed <see cref="VerbResult"/> values via <see cref="SnapshotErrorMapper"/>;
/// everything else propagates so the unhandled-exception handler shows a stack trace.
/// </summary>
internal sealed class SnapshotHandler : ISnapshotHandler
{
    private readonly IDatabaseSnapshotProvider _provider;
    private readonly DbResetOptions _options;

    public SnapshotHandler(IDatabaseSnapshotProvider provider, DbResetOptions options)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(options);

        _provider = provider;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<VerbResult> RunAsync(CancellationToken ct)
    {
        TimeSpan elapsed;
        try
        {
            elapsed = await _provider.CreateSnapshotAsync(ct);
        }
        catch (SqlException ex)
        {
            var mapped = SnapshotErrorMapper.TryMap(ex, _options.SnapshotSource, _options.BaselinePath);
            if (mapped is not null)
            {
                return mapped;
            }
            throw;
        }

        // Size: prefer on-disk if the .bak is reachable from this process; otherwise say so
        // explicitly. Mirrors VerifyBaselineHandler so verify-baseline and snapshot output line
        // up after a successful snapshot.
        string sizeText;
        if (File.Exists(_options.BaselinePath))
        {
            var onDisk = new FileInfo(_options.BaselinePath).Length;
            sizeText = ByteSizeFormatter.Format(onDisk);
        }
        else
        {
            sizeText = "(unknown — .bak not visible)";
        }

        var message = string.Format(
            CultureInfo.InvariantCulture,
            "Snapshot written to {0}: {1} in {2}",
            _options.BaselinePath,
            sizeText,
            FormatElapsed(elapsed));

        return VerbResult.Ok(message);
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        // Operator-friendly: under a minute → seconds with one decimal; otherwise mm:ss.
        if (elapsed.TotalMinutes < 1)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0}s", elapsed.TotalSeconds);
        }
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:00}:{1:00}",
            (int)elapsed.TotalMinutes,
            elapsed.Seconds);
    }
}
