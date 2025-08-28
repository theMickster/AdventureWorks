using System.Data;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Snapshot.Internal;

/// <summary>
/// The only place in the project that directly calls <c>Microsoft.Data.SqlClient</c>. Every other
/// caller goes through <see cref="ISqlScriptExecutor"/>. Stateless and thread-safe; registered as
/// a singleton.
/// </summary>
internal sealed class SqlScriptExecutor : ISqlScriptExecutor
{
    public async Task<SqlConnection> OpenAsync(string connectionString, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(ct);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task ExecuteOnConnectionAsync(
        SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = commandTimeoutSeconds;
        AddParameters(cmd, parameters);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<TResult> ReadOnConnectionAsync<TResult>(
        SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct,
        Func<IDataReader, TResult> projector)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(projector);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = commandTimeoutSeconds;
        AddParameters(cmd, parameters);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return projector(reader);
    }

    public async Task<object?> ScalarOnConnectionAsync(
        SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = commandTimeoutSeconds;
        AddParameters(cmd, parameters);
        return await cmd.ExecuteScalarAsync(ct);
    }

    public async Task ExecuteAsync(
        string connectionString,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct)
    {
        await using var connection = await OpenAsync(connectionString, ct);
        await ExecuteOnConnectionAsync(connection, sql, parameters, commandTimeoutSeconds, ct);
    }

    public async Task<TResult> ReadAsync<TResult>(
        string connectionString,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct,
        Func<IDataReader, TResult> projector)
    {
        await using var connection = await OpenAsync(connectionString, ct);
        return await ReadOnConnectionAsync(connection, sql, parameters, commandTimeoutSeconds, ct, projector);
    }

    public async Task<object?> ScalarAsync(
        string connectionString,
        string sql,
        IReadOnlyList<SqlParameter> parameters,
        int commandTimeoutSeconds,
        CancellationToken ct)
    {
        await using var connection = await OpenAsync(connectionString, ct);
        return await ScalarOnConnectionAsync(connection, sql, parameters, commandTimeoutSeconds, ct);
    }

    private static void AddParameters(SqlCommand cmd, IReadOnlyList<SqlParameter> parameters)
    {
        for (var i = 0; i < parameters.Count; i++)
        {
            cmd.Parameters.Add(parameters[i]);
        }
    }
}
