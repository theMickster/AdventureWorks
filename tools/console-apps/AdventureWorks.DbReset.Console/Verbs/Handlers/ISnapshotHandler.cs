namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Handler abstraction for the <c>snapshot</c> verb. The verb captures a fresh baseline backup
/// from the configured <c>SnapshotSource</c>; it intentionally takes no target argument because
/// snapshot is a source-only operation (see Rule #1 in <c>DualRoleSafetyValidator</c>).
/// </summary>
public interface ISnapshotHandler
{
    /// <summary>
    /// Invokes <see cref="Snapshot.IDatabaseSnapshotProvider.CreateSnapshotAsync"/>, measures
    /// wall-clock duration, and translates known failure modes (source unreachable, OS error 5
    /// while writing the .bak) into operator-friendly <see cref="VerbResult"/> values.
    /// Unrecognized exceptions propagate so the caller sees the full stack trace.
    /// </summary>
    Task<VerbResult> RunAsync(CancellationToken ct);
}
