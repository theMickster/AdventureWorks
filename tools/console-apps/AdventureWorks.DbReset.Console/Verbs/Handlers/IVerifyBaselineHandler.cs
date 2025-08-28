namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Handler abstraction for the <c>verify-baseline</c> verb. The verb reports whether the
/// configured <c>BaselinePath</c> is readable from the target instance's perspective; this
/// indirection lets <c>Program.cs</c> resolve the implementation through the container and
/// keeps the handler unit-testable in isolation from CommandLineParser.
/// </summary>
public interface IVerifyBaselineHandler
{
    /// <summary>
    /// Probes the baseline file via the registered <see cref="Snapshot.IDatabaseSnapshotProvider"/>
    /// and returns a <see cref="VerbResult"/>. Implementations must not write to
    /// <see cref="System.Console"/> directly.
    /// </summary>
    /// <param name="targetName">The resolved target name (post safety + resolver pipeline).</param>
    /// <param name="ct">Cancellation token threaded from <c>Program.cs</c>.</param>
    Task<VerbResult> RunAsync(string targetName, CancellationToken ct);
}
