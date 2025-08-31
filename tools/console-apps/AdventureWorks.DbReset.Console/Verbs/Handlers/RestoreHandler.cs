using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Snapshot;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Default <see cref="IRestoreHandler"/>. Drives
/// <see cref="IDatabaseSnapshotProvider.RestoreSnapshotAsync"/>, times the wall-clock cost, and
/// composes the operator-facing summary. Known failure modes (target unreachable / OS error 5)
/// are mapped to typed <see cref="VerbResult"/> values via <see cref="RestoreErrorMapper"/>;
/// everything else propagates so the unhandled-exception handler shows a stack trace.
/// </summary>
/// <remarks>
/// Safety validation (Rules #1–#5) is performed upstream in <c>Program.Dispatch</c>; this
/// handler never re-validates. Cancellation cleanup is guaranteed at the provider layer —
/// <see cref="IDatabaseSnapshotProvider.RestoreSnapshotAsync"/> runs
/// <c>TryCleanupMultiUserAsync</c> with <see cref="CancellationToken.None"/> before rethrowing,
/// so this handler does not catch <see cref="OperationCanceledException"/>.
/// </remarks>
internal sealed class RestoreHandler : IRestoreHandler
{
    private readonly IDatabaseSnapshotProvider _provider;
    private readonly DbResetOptions _options;

    public RestoreHandler(IDatabaseSnapshotProvider provider, DbResetOptions options)
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
            return VerbResult.Fail(
                DbResetDefaults.ExitVerifyBaselineMissing,
                string.Format(CultureInfo.InvariantCulture, DbResetDefaults.BaselineMissingGuidanceFormat, status.Path));
        }

        TimeSpan elapsed;
        try
        {
            elapsed = await _provider.RestoreSnapshotAsync(targetName, ct);
        }
        catch (SqlException ex)
        {
            var mapped = RestoreErrorMapper.TryMap(ex, targetName, _options.BaselinePath);
            if (mapped is not null) return mapped;
            throw;
        }

        return VerbResult.Ok(string.Format(
            CultureInfo.InvariantCulture,
            "restore complete: {0} in {1}",
            targetName,
            FormatElapsed(elapsed)));
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed.TotalMinutes < 1)
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0}s", elapsed.TotalSeconds);
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", (int)elapsed.TotalMinutes, elapsed.Seconds);
    }
}
