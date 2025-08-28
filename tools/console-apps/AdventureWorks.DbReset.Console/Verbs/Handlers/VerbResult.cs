namespace AdventureWorks.DbReset.Console.Verbs.Handlers;

/// <summary>
/// Captured outcome of a verb handler run. Handlers never touch <see cref="System.Console"/>
/// directly; <c>Program.cs</c> consumes this record to perform stream writes and propagate the
/// process exit code. Returning data instead of writing to streams keeps handlers trivially
/// testable and avoids accidental output coupling between verb logic and presentation.
/// </summary>
/// <param name="ExitCode">Process exit code to return from <c>Main</c>.</param>
/// <param name="StdOut">Optional single-line message for <see cref="System.Console.Out"/>. <c>null</c> means write nothing.</param>
/// <param name="StdErr">Optional single-line message for <see cref="System.Console.Error"/>. <c>null</c> means write nothing.</param>
public sealed record VerbResult(int ExitCode, string? StdOut, string? StdErr)
{
    /// <summary>
    /// Convenience factory for the success path. Sets <see cref="ExitCode"/> to
    /// <see cref="Configuration.DbResetDefaults.ExitOk"/> and leaves <see cref="StdErr"/> null.
    /// </summary>
    public static VerbResult Ok(string? stdOut)
        => new(Configuration.DbResetDefaults.ExitOk, stdOut, null);

    /// <summary>
    /// Convenience factory for failure paths. The caller specifies the exit code (so each
    /// failure mode can map to its own AC-defined code) and the operator-facing error message.
    /// </summary>
    public static VerbResult Fail(int exitCode, string stdErr)
    {
        ArgumentException.ThrowIfNullOrEmpty(stdErr);
        return new VerbResult(exitCode, null, stdErr);
    }
}
