using System.ComponentModel;
using System.Diagnostics;
using AdventureWorks.DbReset.Console.Configuration;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Migration;

/// <summary>
/// Default <see cref="IDbUpProcessRunner"/>. Spawns <c>dotnet run --project &lt;path&gt;</c> as a
/// child process, sets the working directory so DbUp's own config loading works, and injects the
/// target connection string via an environment variable. Stdio is inherited — DbUp output streams
/// verbatim to the operator's terminal.
/// </summary>
internal sealed class DbUpProcessRunner : IDbUpProcessRunner
{
    /// <inheritdoc />
    public async Task<int> RunAsync(string absoluteProjectPath, string connectionString, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absoluteProjectPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var psi = new ProcessStartInfo
        {
            FileName         = "dotnet",
            WorkingDirectory = absoluteProjectPath,
            UseShellExecute  = false,
            // stdio intentionally NOT redirected — DbUp output streams verbatim to the operator's terminal.
        };
        psi.ArgumentList.Add("run");
        psi.ArgumentList.Add("--project");
        psi.ArgumentList.Add(absoluteProjectPath);
        psi.Environment[DbResetDefaults.DbUpConnectionStringEnvVar] = connectionString;
        var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        if (!string.IsNullOrEmpty(databaseName))
            psi.Environment[DbResetDefaults.DbUpDatabaseNameEnvVar] = databaseName;

        using var process = new Process { StartInfo = psi };
        try
        {
            process.Start();
        }
        catch (Exception ex) when (ex is Win32Exception or IOException)
        {
            throw new InvalidOperationException(
                "Failed to launch 'dotnet run' — verify the .NET SDK is on PATH.", ex);
        }

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { }
            throw;
        }

        return process.ExitCode;
    }
}
