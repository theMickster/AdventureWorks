using System.Data;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Snapshot.Internal;

/// <summary>
/// Seam between <see cref="LocalSqlServerSnapshotProvider"/> and
/// <c>Microsoft.Data.SqlClient</c>. Mockable so the orchestrator's cancellation/finally semantics
/// can be exercised without a live SQL Server.
/// </summary>
internal interface ISqlScriptExecutor
{
    /// <summary>Opens a new <see cref="SqlConnection"/>; the caller owns disposal.</summary>
    Task<SqlConnection> OpenAsync(string connectionString, CancellationToken ct);

    /// <summary>Executes a non-query command on an already-open connection.</summary>
    Task ExecuteOnConnectionAsync(
        SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct);

    /// <summary>Reads result rows on an already-open connection and projects them through <paramref name="projector"/>.</summary>
    Task<TResult> ReadOnConnectionAsync<TResult>(
        SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct,
        Func<IDataReader, TResult> projector);

    /// <summary>Single-value read on an already-open connection. Returns the raw scalar (may be <c>null</c> or <see cref="DBNull"/>).</summary>
    Task<object?> ScalarOnConnectionAsync(
        SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct);

    /// <summary>Open + execute non-query + dispose, in one shot.</summary>
    Task ExecuteAsync(
        string connectionString,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct);

    /// <summary>Open + read + dispose, in one shot.</summary>
    Task<TResult> ReadAsync<TResult>(
        string connectionString,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct,
        Func<IDataReader, TResult> projector);

    /// <summary>Open + scalar + dispose, in one shot. Used by <see cref="Safety.SqlSourceMarkerProbe"/>.</summary>
    Task<object?> ScalarAsync(
        string connectionString,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct);
}
