namespace AdventureWorks.DbReset.Console.Configuration;

/// <summary>
/// Tool-wide defaults — process exit codes, repo-root marker filenames, and environment-variable
/// names. Centralized so callers (verbs, tests, CI scripts) can refer to them by symbol rather
/// than magic value.
/// </summary>
internal static class DbResetDefaults
{
    /// <summary>Default exit code for a successful run.</summary>
    public const int ExitOk = 0;

    /// <summary>Exit code returned when CommandLineParser rejects argv (unknown verb, bad flag, etc.).</summary>
    public const int ExitParseError = 1;

    /// <summary>Exit code returned when <see cref="ConfigurationValidator"/> rejects <c>appsettings.json</c>.</summary>
    public const int ExitConfigInvalid = 2;

    /// <summary>Exit code returned when <see cref="Safety.DualRoleSafetyValidator"/> refuses a run (any of Rules #1–#5).</summary>
    public const int ExitSafetyRefused = 3;

    /// <summary>Exit code returned when <see cref="Resolution.RepoRootResolver"/> walks past the filesystem root without finding a marker.</summary>
    public const int ExitRepoRootMissing = 4;

    /// <summary>Primary repo-root marker — a <c>.git</c> directory. Checked first because every clone has one.</summary>
    public const string RepoMarkerGitDir = ".git";

    /// <summary>Fallback repo-root marker — the solution file. Lets the tool work from a source archive that lacks <c>.git</c>.</summary>
    public const string RepoMarkerSolution = "AdventureWorks.sln";

    /// <summary>Standard ASP.NET Core hosting environment variable name. Honored by <c>Microsoft.Extensions.Hosting</c>.</summary>
    public const string EnvVarAspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";

    /// <summary>Environment fallback when <see cref="EnvVarAspNetCoreEnvironment"/> is unset.</summary>
    public const string EnvDefault = "Development";
}
