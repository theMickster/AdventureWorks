using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Translates a subset of <see cref="SqlException"/> failures during <c>RESTORE DATABASE</c>
/// into operator-friendly <see cref="VerbResult"/> values. Recognized cases are OS error 5
/// (SQL Server cannot read the .bak) and target unreachable (network / login / database missing).
/// Unrecognized exceptions return <c>null</c> so the caller can rethrow and surface the full
/// diagnostic.
/// </summary>
/// <remarks>
/// Output messages are intentionally single-paragraph and never include
/// <see cref="Exception.ToString"/> output: operators see this message at the top of a CI log
/// or a failed Docker run and need an immediately-actionable hint, not a stack trace they have
/// to scroll past.
/// </remarks>
internal static class RestoreErrorMapper
{
    // SQL error numbers covering "I cannot reach this database from this connection string".
    // The Number==0 + InnerException branch in IsTargetUnreachable below also catches the
    // SqlClient transport-layer cases that surface without a server-supplied error number.
    private const int SqlErrorCannotOpenDatabase = 4060;   // server is reachable; database name is wrong / offline
    private const int SqlErrorLoginFailed = 18456;          // server reachable; auth rejected
    private const int SqlErrorNetworkConnect = 53;          // network path not found
    private const int SqlErrorServerNotFound = 40;          // could not open a connection
    private const int SqlErrorConnectionTimeout = 10060;    // tcp connect timeout
    private const int SqlErrorTransportLevel = 10054;       // existing connection forcibly closed

    private const string OsErrorFiveMarker = "Operating system error 5";

    /// <summary>
    /// Returns a <see cref="VerbResult"/> describing the failure, or <c>null</c> if
    /// <paramref name="ex"/> is not a recognized restore failure mode and should propagate.
    /// </summary>
    /// <param name="ex">The exception caught by <see cref="RestoreHandler"/>.</param>
    /// <param name="targetName">The <c>ConnectionStrings</c> key for the restore target — included in the message so operators know which entry to fix.</param>
    /// <param name="baselinePath">The configured baseline file path — referenced in the OS-error-5 hint.</param>
    /// <returns>A mapped <see cref="VerbResult"/>, or <c>null</c> if the exception is unrecognized.</returns>
    public static VerbResult? TryMap(SqlException ex, string targetName, string baselinePath)
    {
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        ArgumentException.ThrowIfNullOrWhiteSpace(baselinePath);

        // OS error 5: RESTORE DATABASE cannot read the .bak. This surfaces as a SqlException
        // whose Message contains "Operating system error 5". Check this BEFORE the network
        // branch — an OS error 5 may surface with Number=3201 or similar non-network values.
        if (ex.Message.Contains(OsErrorFiveMarker, StringComparison.OrdinalIgnoreCase))
        {
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                "SQL Server cannot read baseline file at '{0}' (OS error 5). The SQL Server service account must have read access to this directory. See DOCKER.md for setup guidance.",
                baselinePath);
            return VerbResult.Fail(DbResetDefaults.ExitRestorePermissionDenied, msg);
        }

        // Target unreachable: the restore destination instance is unreachable. We name the
        // target key (not the raw connection string — that may contain credentials) so the
        // operator knows which appsettings / User Secrets entry to fix.
        if (IsTargetUnreachable(ex))
        {
            var reason = SummarizeReason(ex);
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                "Cannot reach restore target '{0}': {1}. Verify the connection string and that the SQL instance is running.",
                targetName,
                reason);
            return VerbResult.Fail(DbResetDefaults.ExitRestoreTargetUnreachable, msg);
        }

        return null;
    }

    private static bool IsTargetUnreachable(SqlException ex)
    {
        if (ex.Number is SqlErrorCannotOpenDatabase
            or SqlErrorLoginFailed
            or SqlErrorNetworkConnect
            or SqlErrorServerNotFound
            or SqlErrorConnectionTimeout
            or SqlErrorTransportLevel)
        {
            return true;
        }

        // SqlClient transport failures sometimes surface as Number==0 with an inner Win32 cause.
        if (ex.Number == 0 && ex.InnerException is not null)
        {
            return true;
        }

        return false;
    }

    private static string SummarizeReason(SqlException ex)
    {
        if (ex.Number != 0)
        {
            return string.Format(CultureInfo.InvariantCulture, "SQL error {0}", ex.Number);
        }
        // Number==0 is a SqlClient transport-layer failure. The driver message can contain
        // the server hostname and port, so we substitute a fixed safe string rather than
        // echoing it to operator-facing output.
        return "transport-level connection failure";
    }
}
