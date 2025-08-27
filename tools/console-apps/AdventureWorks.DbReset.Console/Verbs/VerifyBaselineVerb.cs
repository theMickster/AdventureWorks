using CommandLine;

namespace AdventureWorks.DbReset.Console.Verbs;

[Verb("verify-baseline", HelpText = "Verifies the baseline backup file exists and is readable.")]
public sealed class VerifyBaselineVerb
{
    // No options for #924; future stories may add --baseline override.
}
