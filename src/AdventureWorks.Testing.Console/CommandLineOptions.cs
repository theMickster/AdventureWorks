using CommandLine;

namespace AdventureWorks.Testing.Console;

internal sealed class CommandLineOptions
{
    [Option('m', "testSecurityModel", Required = false, HelpText = "Execute a quick test of the AdventureWorks security model.")]
    public bool TestSecurityModel { get; set; }
}
