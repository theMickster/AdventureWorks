namespace AdventureWorks.DbReset.Console.Migration;

/// <summary>
/// Abstraction over the <c>dotnet run</c> child process that drives AdventureWorks.DbUp.
/// Exists to keep <see cref="Verbs.Handlers.MigrateHandler"/> unit-testable without spawning a real process.
/// </summary>
internal interface IDbUpProcessRunner
{
    /// <summary>
    /// Spawns <c>dotnet run --project <paramref name="absoluteProjectPath"/></c> with the
    /// target connection string injected via <c>ConnectionStrings__AdventureWorks</c>. Inherits
    /// the parent's stdio handles so DbUp output streams verbatim. Returns the child process exit code.
    /// </summary>
    /// <param name="absoluteProjectPath">Absolute path to the DbUp project directory.</param>
    /// <param name="connectionString">Resolved target connection string forwarded to DbUp.</param>
    /// <param name="ct">Cancellation token; cancellation kills the child process tree.</param>
    Task<int> RunAsync(string absoluteProjectPath, string connectionString, CancellationToken ct);
}
