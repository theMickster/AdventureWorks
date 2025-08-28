namespace AdventureWorks.DbReset.Console.Snapshot;

/// <summary>
/// Strategy abstraction for snapshot/restore orchestration. Verbs depend on this interface,
/// never on a concrete provider. Implementations are stateless and may be registered as
/// singletons.
/// </summary>
/// <remarks>
/// Introduced for ADO Feature #923 / Story #925. The shipping concrete is
/// <see cref="LocalSqlServerSnapshotProvider"/>, targeting a local/Docker SQL Server via
/// <c>Microsoft.Data.SqlClient</c>.
/// </remarks>
public interface IDatabaseSnapshotProvider
{
    /// <summary>
    /// Probes the configured baseline file via <c>RESTORE FILELISTONLY</c> against the target
    /// instance's <c>[master]</c>. Returns <see cref="BaselineStatus.Missing(string)"/> for the
    /// SQL errors <c>3201</c> (file not found), <c>3203</c> (read failure), and <c>3241</c>
    /// (not a valid backup); other <see cref="Microsoft.Data.SqlClient.SqlException"/>s propagate.
    /// </summary>
    /// <param name="targetName">The <c>ConnectionStrings</c> key naming the target database.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<BaselineStatus> BaselineExistsAsync(string targetName, CancellationToken ct);

    /// <summary>
    /// Creates the baseline snapshot from the configured <c>SnapshotSource</c>. The source is
    /// fixed; this method intentionally does NOT take a target. Two phases:
    /// (1) <c>BACKUP DATABASE</c> against the source's <c>[master]</c>, (2) stamp the
    /// <c>SourceMarker</c> extended property on the source database itself.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Wall-clock duration of the operation.</returns>
    Task<TimeSpan> CreateSnapshotAsync(CancellationToken ct);

    /// <summary>
    /// Restores the baseline into the named target. Performs the
    /// <c>SINGLE_USER → RESTORE → MULTI_USER → RECOVERY SIMPLE → drop-marker</c> sequence on a
    /// single non-pooled connection. Cancellation triggers a best-effort
    /// <c>SET MULTI_USER</c> recovery on a fresh, untrackable token before the original
    /// exception is rethrown.
    /// </summary>
    /// <param name="targetName">The <c>ConnectionStrings</c> key naming the target database.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Wall-clock duration of the operation.</returns>
    Task<TimeSpan> RestoreSnapshotAsync(string targetName, CancellationToken ct);

    /// <summary>
    /// Read-only inspection of the target's state followed by minimum corrective ALTERs:
    /// recovers from <c>RESTORING</c> (state=1) and / or restores <c>MULTI_USER</c> from
    /// <c>SINGLE_USER</c> (user_access=1). Idempotent — when neither condition is present, no
    /// SQL is issued beyond the diagnostic SELECT.
    /// </summary>
    /// <param name="targetName">The <c>ConnectionStrings</c> key naming the target database.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<RecoveryOutcome> SelfHealStuckTargetAsync(string targetName, CancellationToken ct);
}
