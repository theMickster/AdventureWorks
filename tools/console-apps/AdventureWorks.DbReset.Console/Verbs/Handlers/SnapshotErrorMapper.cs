using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Translates a subset of <see cref="SqlException"/> failures during <c>BACKUP DATABASE</c> /
/// <c>xp_create_subdir</c> into operator-friendly <see cref="VerbResult"/> values. Recognized
/// cases are AC #4 (source unreachable / DB missing) and AC #5 (filesystem permission denied
/// while writing the .bak). Unrecognized exceptions return <c>null</c> so the caller can
/// rethrow and surface the full diagnostic.
/// </summary>
/// <remarks>
/// Output messages are intentionally single-paragraph and never include
/// <see cref="Exception.ToString"/> output: operators see this message at the top of a CI log
/// or a failed Docker run and need an immediately-actionable hint, not a stack trace they have
/// to scroll past.
/// </remarks>
internal static class SnapshotErrorMapper
{
    // SQL error numbers covering "I cannot reach this database from this connection string".
    // The Number==0 + InnerException branch in IsSourceUnreachable below also catches the
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
    /// <paramref name="ex"/> is not a recognized snapshot failure mode and should propagate.
    /// </summary>
    /// <param name="ex">The exception caught by <see cref="SnapshotHandler"/>.</param>
    /// <param name="snapshotSourceKey">The <c>ConnectionStrings</c> key for the source — included in the message so operators know which entry to fix.</param>
    /// <param name="baselinePath">The configured baseline file path — referenced in the OS-error-5 hint.</param>
    public static VerbResult? TryMap(SqlException ex, string snapshotSourceKey, string baselinePath)
    {
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotSourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(baselinePath);

        // AC #5: BACKUP / xp_create_subdir wraps the OS error in a SqlException whose Message
        // contains "Operating system error 5". This covers both directory-create perm failures
        // and the actual BACKUP DATABASE write. Check this BEFORE the network branch — an OS
        // error 5 may surface with Number=3201 or similar non-network values.
        if (ex.Message.Contains(OsErrorFiveMarker, StringComparison.OrdinalIgnoreCase))
        {
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                "Permission denied writing baseline file at '{0}' (OS error 5). The SQL Server service account must own (or have write access to) this directory. See DOCKER.md for setup guidance.",
                baselinePath);
            return VerbResult.Fail(DbResetDefaults.ExitSnapshotPermissionDenied, msg);
        }

        // AC #4: the source database / instance is unreachable. We name the source key (not the
        // raw connection string — that may contain credentials) so the operator knows which
        // appsettings / User Secrets entry to fix.
        if (IsSourceUnreachable(ex))
        {
            var reason = SummarizeReason(ex);
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                "Cannot reach SnapshotSource '{0}': {1}. Verify the connection string and that the SQL instance is running.",
                snapshotSourceKey,
                reason);
            return VerbResult.Fail(DbResetDefaults.ExitSnapshotSourceUnreachable, msg);
        }

        return null;
    }

    private static bool IsSourceUnreachable(SqlException ex)
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

    /// <summary>
    /// Returns a short, human-readable phrase describing the failure: prefer the SQL error
    /// number when present, fall back to the first sentence of the exception message. We never
    /// dump the full message because SqlClient sometimes appends multi-line driver detail.
    /// </summary>
    private static string SummarizeReason(SqlException ex)
    {
        if (ex.Number != 0)
        {
            return string.Format(CultureInfo.InvariantCulture, "SQL error {0}", ex.Number);
        }
        var firstLine = ex.Message.Split('\n', 2, StringSplitOptions.None)[0].Trim();
        return string.IsNullOrEmpty(firstLine) ? "connection failed" : firstLine;
    }
}
