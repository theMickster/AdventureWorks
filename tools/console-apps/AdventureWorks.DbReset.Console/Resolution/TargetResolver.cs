namespace AdventureWorks.DbReset.Console.Resolution;

/// <summary>
/// Picks the effective target <c>ConnectionStrings</c> key for a verb, preferring an explicit
/// <c>--target</c> CLI override over the configured <c>DbReset:DefaultTarget</c>.
/// </summary>
public sealed class TargetResolver
{
    /// <summary>
    /// Returns <paramref name="cliTarget"/> when supplied (non-null, non-whitespace); otherwise
    /// falls back to <paramref name="defaultTarget"/>.
    /// </summary>
    /// <param name="cliTarget">Optional CLI override. <c>null</c> or whitespace means "use the default".</param>
    /// <param name="defaultTarget">Configured fallback (typically <c>DbReset:DefaultTarget</c>).</param>
    /// <returns>The effective target key passed downstream to the safety validator and verbs.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="defaultTarget"/> is <c>null</c>.</exception>
    public string Resolve(string? cliTarget, string defaultTarget)
    {
        ArgumentNullException.ThrowIfNull(defaultTarget);
        return string.IsNullOrWhiteSpace(cliTarget) ? defaultTarget : cliTarget;
    }
}
