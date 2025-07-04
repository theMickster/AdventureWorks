using DbUp;
using DbUp.Engine;
using System.Reflection;

namespace AdventureWorks.DbUp.Deployers;

internal sealed class MigrationDeployer
{
    private readonly string _connectionString;

    public MigrationDeployer(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public DatabaseUpgradeResult Deploy()
    {
        Console.WriteLine("Running one-time migrations (Scripts folder)...");
        Console.WriteLine("─────────────────────────────────────────────────");

        var upgrader = DeployChanges.To
            .SqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                script => script.Contains(".Scripts."))
            .JournalToSqlTable("dbo", "DatabaseMigrationHistory")
            .LogToConsole()
            .WithTransactionPerScript()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Migrations completed successfully!");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Migration failed: {result.Error}");
            Console.ResetColor();
        }

        return result;
    }
}
