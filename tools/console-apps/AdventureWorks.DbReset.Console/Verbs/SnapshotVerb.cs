using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

[Verb("snapshot", HelpText = "Captures a fresh baseline backup from SnapshotSource.")]
public sealed class SnapshotVerb
{
    // No --target by design: snapshot only reads from SnapshotSource.
}
