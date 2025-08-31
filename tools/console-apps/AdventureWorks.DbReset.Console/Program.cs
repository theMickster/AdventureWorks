using System.Reflection;
using AdventureWorks.DbReset.Console.Configuration;
using AdventureWorks.DbReset.Console.Libs;
using AdventureWorks.DbReset.Console.Resolution;
using AdventureWorks.DbReset.Console.Safety;
using AdventureWorks.DbReset.Console.Verbs;
using AdventureWorks.DbReset.Console.Verbs.Handlers;
using Autofac;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace AdventureWorks.DbReset.Console;

internal static class Program
{
    private static int Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable(DbResetDefaults.EnvVarAspNetCoreEnvironment)
            ?? DbResetDefaults.EnvDefault;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(typeof(Program).Assembly, optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var options = new DbResetOptions();
        configuration.GetSection(DbResetOptions.SectionName).Bind(options);

        var connectionStrings = configuration
            .GetSection("ConnectionStrings")
            .GetChildren()
            .ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);

        var configValidator = new ConfigurationValidator();
        var configResult = configValidator.Validate(options, connectionStrings);
        if (!configResult.Ok)
        {
            System.Console.Error.WriteLine(configResult.Reason);
            return DbResetDefaults.ExitConfigInvalid;
        }

        // Verify the repo-root marker is reachable from the running binary.
        // Future verb logic depends on this; failing fast here gives a clear message
        // before any verb stub runs.
        var repoRoot = new RepoRootResolver().Resolve(AppContext.BaseDirectory);
        if (repoRoot is null)
        {
            System.Console.Error.WriteLine(
                $"Repository root not found walking up from '{AppContext.BaseDirectory}'. "
                + $"Expected one of: '{DbResetDefaults.RepoMarkerGitDir}' (directory) or "
                + $"'{DbResetDefaults.RepoMarkerSolution}' (file).");
            return DbResetDefaults.ExitRepoRootMissing;
        }

        using var container = AutofacBootstrapper.Build(configuration, options, repoRoot);

        // CancellationTokenSource + CancelKeyPress are wired so Story #927's restore handler can
        // thread the token through async SQL paths without refactoring scaffolding. #924 stubs do
        // no async work, so cts.Token is intentionally unobserved at the dispatch boundary today.
        using var cts = new CancellationTokenSource();
        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        return Parser.Default.ParseArguments<
                VerifyBaselineVerb, SnapshotVerb, RestoreVerb, MigrateVerb, ResetVerb>(args)
            .MapResult(
                (VerifyBaselineVerb v) => Dispatch(container, options, connectionStrings, v, cts.Token),
                (SnapshotVerb v) => Dispatch(container, options, connectionStrings, v, cts.Token),
                (RestoreVerb v) => Dispatch(container, options, connectionStrings, v, cts.Token),
                (MigrateVerb v) => Dispatch(container, options, connectionStrings, v, cts.Token),
                (ResetVerb v) => Dispatch(container, options, connectionStrings, v, cts.Token),
                _ => DbResetDefaults.ExitParseError);
    }

    private static int Dispatch<TVerb>(
        IContainer container,
        DbResetOptions options,
        IReadOnlyDictionary<string, string?> connectionStrings,
        TVerb verb,
        CancellationToken ct)
        where TVerb : class
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(connectionStrings);
        ArgumentNullException.ThrowIfNull(verb);

        var verbName = typeof(TVerb).GetCustomAttribute<VerbAttribute>()?.Name ?? typeof(TVerb).Name;
        var cliTarget = (verb as TargetableVerb)?.Target;

        var targetResolver = container.Resolve<TargetResolver>();
        var effectiveTarget = targetResolver.Resolve(cliTarget, options.DefaultTarget);

        // Snapshot reads only from SnapshotSource — there's no target to safety-check. Every
        // other verb writes to the target and must clear the dual-role validator first.
        if (verb is not SnapshotVerb)
        {
            var safetyValidator = container.Resolve<DualRoleSafetyValidator>();
            var outcome = safetyValidator.Validate(options, effectiveTarget, connectionStrings);
            if (!outcome.Ok)
            {
                System.Console.Error.WriteLine($"{outcome.FailedRule}: {outcome.Reason}");
                return DbResetDefaults.ExitSafetyRefused;
            }
        }

        return verb switch
        {
            VerifyBaselineVerb => RunHandler(
                () => container.Resolve<IVerifyBaselineHandler>().RunAsync(effectiveTarget, ct)),
            SnapshotVerb => RunHandler(
                () => container.Resolve<ISnapshotHandler>().RunAsync(ct)),
            RestoreVerb => RunHandler(
                () => container.Resolve<IRestoreHandler>().RunAsync(effectiveTarget, ct)),
            MigrateVerb => RunHandler(
                () => container.Resolve<IMigrateHandler>().RunAsync(effectiveTarget, ct)),
            ResetVerb => RunHandler(
                () => container.Resolve<IResetHandler>().RunAsync(effectiveTarget, ct)),
            _ => RunStub(verbName, effectiveTarget),
        };
    }

    /// <summary>
    /// Bridges the synchronous CommandLineParser <c>MapResult</c> world to async handlers.
    /// Handlers return a <see cref="VerbResult"/>; we print its streams here so handlers stay
    /// free of any direct <see cref="System.Console"/> coupling.
    /// </summary>
    private static int RunHandler(Func<Task<VerbResult>> run)
    {
        try
        {
            var result = run().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(result.StdOut))
            {
                System.Console.Out.WriteLine(result.StdOut);
            }
            if (!string.IsNullOrEmpty(result.StdErr))
            {
                System.Console.Error.WriteLine(result.StdErr);
            }
            return result.ExitCode;
        }
        catch (OperationCanceledException)
        {
            System.Console.Error.WriteLine("Operation cancelled.");
            return DbResetDefaults.ExitCancelled;
        }
    }

    private static int RunStub(string verbName, string effectiveTarget)
    {
        System.Console.WriteLine($"stub: {verbName} (target={effectiveTarget})");
        return DbResetDefaults.ExitOk;
    }
}
