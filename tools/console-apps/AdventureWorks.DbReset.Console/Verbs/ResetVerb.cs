using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

[Verb("reset", HelpText = "Convenience: restore + migrate against the target.")]
public sealed class ResetVerb : TargetableVerb
{
}
