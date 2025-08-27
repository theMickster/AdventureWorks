using AdventureWorks.DbReset.Console.Configuration;

namespace AdventureWorks.DbReset.Console.Resolution;

/// <summary>
/// Walks up from a starting directory looking for a marker that identifies the AdventureWorks
/// repository root. Used to anchor relative paths in <see cref="DbResetOptions"/>
/// (<c>BaselinePath</c>, <c>DbUpProjectPath</c>) so the tool works regardless of where it is
/// invoked from.
/// </summary>
/// <remarks>
/// Two markers are checked at each level: a <c>.git</c> directory (the canonical clone marker)
/// and the solution file (<see cref="DbResetDefaults.RepoMarkerSolution"/>). Both are accepted
/// because some deployment scenarios — source archives, container layers — preserve the solution
/// file but strip <c>.git</c>. The <c>.git</c> check is first because it's nearly always faster
/// to satisfy in a real clone.
/// </remarks>
public sealed class RepoRootResolver
{
    /// <summary>
    /// Walks up from <paramref name="startDirectory"/> until either a marker is found or the
    /// filesystem root is reached.
    /// </summary>
    /// <param name="startDirectory">Directory to begin searching from (typically <c>AppContext.BaseDirectory</c>).</param>
    /// <returns>The absolute path of the discovered repo root, or <c>null</c> when no marker is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="startDirectory"/> is <c>null</c>.</exception>
    public string? Resolve(string startDirectory)
    {
        ArgumentNullException.ThrowIfNull(startDirectory);

        var current = new DirectoryInfo(startDirectory);

        while (current is not null)
        {
            // .git can also be a file in a worktree (gitdir: ...). Story #924 only
            // requires the directory check; revisit if worktrees become a concern.
            if (Directory.Exists(Path.Combine(current.FullName, DbResetDefaults.RepoMarkerGitDir)))
            {
                return current.FullName;
            }

            if (File.Exists(Path.Combine(current.FullName, DbResetDefaults.RepoMarkerSolution)))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}
