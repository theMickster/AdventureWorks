namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Handler abstraction for the <c>restore</c> verb. The verb resets the target database to the
/// configured baseline by delegating the SINGLE_USER → RESTORE → MULTI_USER → RECOVERY SIMPLE
/// sequence to <see cref="Snapshot.IDatabaseSnapshotProvider.RestoreSnapshotAsync"/>.
/// Safety rules (#1–#5) are enforced upstream in <c>Program.Dispatch</c>; this handler never
/// re-validates them.
/// </summary>
public interface IRestoreHandler
{
    /// <summary>
    /// Proactively checks baseline existence, then drives
    /// <see cref="Snapshot.IDatabaseSnapshotProvider.RestoreSnapshotAsync"/>. Known failure modes
    /// (target unreachable, OS error 5 reading the .bak) are translated into operator-friendly
    /// <see cref="VerbResult"/> values via <see cref="RestoreErrorMapper"/>. Unrecognized
    /// <see cref="Microsoft.Data.SqlClient.SqlException"/> instances and any
    /// <see cref="OperationCanceledException"/> propagate — the provider's catch block has
    /// already run <c>TryCleanupMultiUserAsync</c> with <see cref="CancellationToken.None"/>
    /// before re-throwing, so cleanup is guaranteed at the provider layer.
    /// </summary>
    /// <param name="targetName">The resolved target <c>ConnectionStrings</c> key.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<VerbResult> RunAsync(string targetName, CancellationToken ct);
}
