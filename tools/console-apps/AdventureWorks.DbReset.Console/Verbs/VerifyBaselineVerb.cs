using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

/// <summary>
/// CLI binding for <c>verify-baseline</c>. Inherits <c>--target</c> from
/// <see cref="TargetableVerb"/> because the readability probe runs against the target
/// instance's <c>[master]</c> via <c>RESTORE FILELISTONLY</c>: SQL Server must be able to read
/// the .bak from the target's perspective, not just from the operator's machine.
/// </summary>
[Verb("verify-baseline", HelpText = "Verifies the baseline backup file exists and is readable from the target instance.")]
public sealed class VerifyBaselineVerb : TargetableVerb
{
}
