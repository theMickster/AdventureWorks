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
/// <b>Fail-closed:</b> a <see cref="SqlException"/> propagates to the caller — except for SQL
/// error 4060 ("cannot open database"), which means the target database does not yet exist on
/// this instance. A non-existent database cannot carry the source marker, so the method returns
/// <c>false</c> and lets the restore proceed to create it. All other <see cref="SqlException"/>
/// values propagate so the validator never silently degrades to "no marker" for ambiguous
/// failures (network, permissions, timeouts).
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

        object? scalar;
        try
        {
            scalar = _executor
                .ScalarAsync(connectionString, sql, parameters, CommandTimeoutSeconds, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }
        catch (SqlException ex) when (ex.Number == 4060)
        {
            // Target database does not exist on this instance. A non-existent DB cannot carry
            // the source marker — return false so the validator clears Rule #5 and the restore
            // proceeds to create the database.
            return false;
        }

        return scalar is not null and not DBNull;
    }
}
