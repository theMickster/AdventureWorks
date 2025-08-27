using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

/// <summary>
/// Base for verbs that operate against a destructive test target. Carries the optional
/// <c>--target</c> override; verb subclasses pick up the option without re-declaring it.
/// </summary>
public abstract class TargetableVerb
{
    /// <summary>
    /// Optional override for the configured <c>DbReset:DefaultTarget</c>. Must satisfy
    /// <c>DbReset:TargetNamePattern</c>; otherwise the safety validator refuses the verb.
    /// </summary>
    [Option('t', "target", Required = false,
        HelpText = "Override the configured DefaultTarget. Must match TargetNamePattern.")]
    public string? Target { get; set; }
}
