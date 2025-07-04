using AdventureWorks.DbUp.Configuration;
using AdventureWorks.DbUp.Deployers;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  AdventureWorks Database Deployment (DbUp)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");

try
{
    var configuration = DbConnectionProvider.BuildConfiguration();
    var connectionProvider = new DbConnectionProvider(configuration);
    var connectionString = connectionProvider.GetConnectionString();
    var databaseName = configuration["DatabaseName"] ?? "AdventureWorks";

    Console.WriteLine($"Target Database: {databaseName}\n");

    // Step 1: Run one-time migrations
    var migrationDeployer = new MigrationDeployer(connectionString);
    var migrationResult = migrationDeployer.Deploy();

    if (!migrationResult.Successful)
    {
        return -1;
    }

    // Step 2: Deploy programmable objects (always run)
    var programmableDeployer = new ProgrammableObjectsDeployer(connectionString);
    var programmableResult = programmableDeployer.Deploy();

    if (!programmableResult.Successful)
    {
        return -1;
    }

    Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  ✓ Database deployment completed successfully!");
    Console.ResetColor();
    Console.WriteLine("═══════════════════════════════════════════════════════════════");

    return 0;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n✗ Fatal error: {ex.Message}");
    Console.ResetColor();
    return -1;
}

