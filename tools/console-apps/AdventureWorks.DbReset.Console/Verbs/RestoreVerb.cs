using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

[Verb("restore", HelpText = "Restores the target database from baseline.")]
public sealed class RestoreVerb : TargetableVerb
{
}
