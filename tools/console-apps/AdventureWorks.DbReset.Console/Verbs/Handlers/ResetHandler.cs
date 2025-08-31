using System.Diagnostics;
using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Default <see cref="IResetHandler"/>. Orchestrates verify-baseline → restore → migrate in
/// order, writing each successful step's output to <see cref="Console.Out"/> for live operator
/// feedback. Returns the first failing <see cref="VerbResult"/> unchanged, or a total-elapsed
/// summary on success.
/// </summary>
/// <remarks>
/// This is an orchestrator handler — unlike leaf handlers (<see cref="RestoreHandler"/>,
/// <see cref="MigrateHandler"/>), it writes intermediate step output to <see cref="Console.Out"/>
/// directly so that verify-baseline and restore messages appear in real time rather than being
/// buffered until the final result is returned.
/// Safety validation (Rules #1–#5) is performed upstream in <c>Program.Dispatch</c>.
/// </remarks>
internal sealed class ResetHandler : IResetHandler
{
    private readonly IVerifyBaselineHandler _verifyBaseline;
    private readonly IRestoreHandler _restore;
    private readonly IMigrateHandler _migrate;

    public ResetHandler(
        IVerifyBaselineHandler verifyBaseline,
        IRestoreHandler restore,
        IMigrateHandler migrate)
    {
        ArgumentNullException.ThrowIfNull(verifyBaseline);
        ArgumentNullException.ThrowIfNull(restore);
        ArgumentNullException.ThrowIfNull(migrate);
        _verifyBaseline = verifyBaseline;
        _restore        = restore;
        _migrate        = migrate;
    }

    /// <inheritdoc />
    public async Task<VerbResult> RunAsync(string targetName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);

        var sw = Stopwatch.StartNew();

        var verifyResult = await _verifyBaseline.RunAsync(targetName, ct);
        if (verifyResult.ExitCode != DbResetDefaults.ExitOk)
            return verifyResult;
        System.Console.Out.WriteLine(verifyResult.StdOut);

        var restoreResult = await _restore.RunAsync(targetName, ct);
        if (restoreResult.ExitCode != DbResetDefaults.ExitOk)
            return restoreResult;
        System.Console.Out.WriteLine(restoreResult.StdOut);

        var migrateResult = await _migrate.RunAsync(targetName, ct);
        if (migrateResult.ExitCode != DbResetDefaults.ExitOk)
            return migrateResult;
        System.Console.Out.WriteLine(migrateResult.StdOut);

        sw.Stop();
        return VerbResult.Ok(
            string.Format(CultureInfo.InvariantCulture,
                "reset complete: {0} in {1}", targetName, FormatElapsed(sw.Elapsed)));
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed.TotalMinutes < 1)
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0}s", elapsed.TotalSeconds);
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", (int)elapsed.TotalMinutes, elapsed.Seconds);
    }
}
