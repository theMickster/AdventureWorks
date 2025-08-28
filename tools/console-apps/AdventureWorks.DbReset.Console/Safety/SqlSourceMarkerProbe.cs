using AdventureWorks.DbReset.Console.Snapshot.Internal;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Safety;

/// <summary>
/// Real (non-stub) implementation of <see cref="ISourceMarkerProbe"/>. Reads
/// <c>sys.extended_properties</c> on the target database and reports whether the configured
/// source-role property is present.
/// </summary>
/// <remarks>
/// <para>
/// <b>Sync-over-async:</b> the surrounding <see cref="DualRoleSafetyValidator.Validate"/> is
/// synchronous, so this method bridges via <c>.GetAwaiter().GetResult()</c>. This is the only
/// approved sync-over-async crossing in the project; deadlock risk is nil because (a) the
/// validator runs once at process startup, before any verb work, and (b) the console host has
/// no <see cref="SynchronizationContext"/>. A future refactor will flip
/// <see cref="ISourceMarkerProbe.HasMarker"/> async and remove this bridge.
/// </para>
/// <para>
/// <b>Fail-closed:</b> a <see cref="SqlException"/> propagates to the caller. The validator must
/// not silently degrade to "no marker" — that would let a destructive verb run against a database
/// whose marker we couldn't read.
/// </para>
/// </remarks>
internal sealed class SqlSourceMarkerProbe : ISourceMarkerProbe
{
    private const int CommandTimeoutSeconds = 30;
    private readonly ISqlScriptExecutor _executor;

    public SqlSourceMarkerProbe(ISqlScriptExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);
        _executor = executor;
    }

    public bool HasMarker(string connectionString, string property, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(property);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        const string sql = """
            SELECT 1
            FROM sys.extended_properties
            WHERE class = 0 AND major_id = 0 AND minor_id = 0
              AND [name] = @property
              AND CAST([value] AS NVARCHAR(MAX)) = @value
            """;

        var parameters = new[]
        {
            new SqlParameter("@property", property),
            new SqlParameter("@value", value),
        };

        var scalar = _executor
            .ScalarAsync(connectionString, sql, parameters, CommandTimeoutSeconds, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return scalar is not null and not DBNull;
    }
}
