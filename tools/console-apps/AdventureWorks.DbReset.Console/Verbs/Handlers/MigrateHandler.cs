using System.Globalization;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Migration;
using Microsoft.Extensions.Configuration;

namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Default <see cref="IMigrateHandler"/>. Resolves the DbUp project path, looks up the target
/// connection string, and drives <see cref="IDbUpProcessRunner.RunAsync"/>. DbUp's stdout and
/// stderr stream verbatim; this handler never captures or parses them.
/// </summary>
/// <remarks>
/// Safety validation (Rules #1–#5) is performed upstream in <c>Program.Dispatch</c>; this
/// handler never re-validates. <see cref="OperationCanceledException"/> is not caught —
/// <see cref="IDbUpProcessRunner"/> kills the child process tree before rethrowing.
/// </remarks>
internal sealed class MigrateHandler : IMigrateHandler
{
    private readonly IDbUpProcessRunner _runner;
    private readonly IConfiguration _configuration;
    private readonly DbResetOptions _options;
    private readonly string _repoRoot;

    public MigrateHandler(
        IDbUpProcessRunner runner,
        IConfiguration configuration,
        DbResetOptions options,
        string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(runner);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(repoRoot);
        _runner        = runner;
        _configuration = configuration;
        _options       = options;
        _repoRoot      = repoRoot;
    }

    /// <inheritdoc />
    public async Task<VerbResult> RunAsync(string targetName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);

        var connectionString = _configuration.GetConnectionString(targetName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "ConnectionStrings:{0} not configured.", targetName));
        }

        var absoluteProjectPath = Path.GetFullPath(Path.Combine(_repoRoot, _options.DbUpProjectPath));
        var repoRootNormalized  = _repoRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                  + Path.DirectorySeparatorChar;
        if (!absoluteProjectPath.StartsWith(repoRootNormalized, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "DbReset:DbUpProjectPath must resolve to a path within the repository root.");
        }

        var exitCode = await _runner.RunAsync(absoluteProjectPath, connectionString, ct);

        if (exitCode == DbResetDefaults.ExitOk)
        {
            return VerbResult.Ok(
                string.Format(CultureInfo.InvariantCulture,
                    "migrate complete: {0}", targetName));
        }

        return VerbResult.Fail(
            DbResetDefaults.ExitMigrateFailed,
            string.Format(CultureInfo.InvariantCulture,
                "migrate failed for '{0}' (DbUp exit code {1}). Review DbUp output above.",
                targetName, exitCode));
    }
}
