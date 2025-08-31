namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Handler abstraction for the <c>reset</c> verb. The verb orchestrates the canonical pre-test
/// cycle: verify-baseline → restore → migrate in order against the resolved target.
/// Safety rules (#1–#5) are enforced upstream in <c>Program.Dispatch</c>; this handler never
/// re-validates them.
/// </summary>
public interface IResetHandler
{
    /// <summary>
    /// Executes verify-baseline, restore, and migrate in sequence against
    /// <paramref name="targetName"/>. Aborts on the first non-zero exit code and returns that
    /// step's <see cref="VerbResult"/> unchanged. On success, returns total elapsed time.
    /// Intermediate per-step messages are written directly to <see cref="Console.Out"/> as each
    /// step completes so the operator sees live progress.
    /// </summary>
    /// <param name="targetName">The resolved target <c>ConnectionStrings</c> key.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<VerbResult> RunAsync(string targetName, CancellationToken ct);
}
