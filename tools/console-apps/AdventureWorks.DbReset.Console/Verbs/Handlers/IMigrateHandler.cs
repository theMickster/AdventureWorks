namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Handler abstraction for the <c>migrate</c> verb. The verb applies DbUp migrations to the
/// target database by shelling out to the AdventureWorks.DbUp project via <c>dotnet run</c>.
/// Safety rules (#1–#5) are enforced upstream in <c>Program.Dispatch</c>; this handler never
/// re-validates them.
/// </summary>
public interface IMigrateHandler
{
    /// <summary>
    /// Resolves the DbUp project path, injects the resolved target connection string as
    /// <c>ConnectionStrings__AdventureWorks</c>, and drives the child process. DbUp's stdout
    /// and stderr stream verbatim through inherited console handles. The verb's exit code mirrors
    /// DbUp's exit code — non-zero maps to <see cref="Configuration.DbResetDefaults.ExitMigrateFailed"/>.
    /// <see cref="OperationCanceledException"/> propagates — the child process is killed before
    /// the exception is rethrown.
    /// </summary>
    /// <param name="targetName">The resolved target <c>ConnectionStrings</c> key.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<VerbResult> RunAsync(string targetName, CancellationToken ct);
}
