using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

[Verb("migrate", HelpText = "Runs DbUp migrations against the target.")]
public sealed class MigrateVerb : TargetableVerb
{
}
