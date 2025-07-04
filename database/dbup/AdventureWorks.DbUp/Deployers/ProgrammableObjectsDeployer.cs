using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using DbUp.Support;
using System.Reflection;

namespace AdventureWorks.DbUp.Deployers;

internal sealed class ProgrammableObjectsDeployer(string connectionString)
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public DatabaseUpgradeResult Deploy()
    {
        Console.WriteLine("\nDeploying programmable objects (ProgrammableObjects folder)...");
        Console.WriteLine("───────────────────────────────────────────────────────────────");

        var upgrader = DeployChanges.To
            .SqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                script => script.Contains(".ProgrammableObjects."),
                new SqlScriptOptions { ScriptType = ScriptType.RunAlways, RunGroupOrder = 1 })
            .JournalTo(new NullJournal())
            .LogToConsole()
            .WithTransactionPerScript()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Programmable objects deployed successfully!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Programmable object deployment failed: {result.Error}");
        }

        Console.ResetColor();

        return result;
    }
}
