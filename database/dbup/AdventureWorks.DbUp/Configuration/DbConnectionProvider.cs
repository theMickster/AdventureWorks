using AdventureWorks.Connections;
using Microsoft.Extensions.Configuration;

namespace AdventureWorks.DbUp.Configuration;

internal sealed class DbConnectionProvider
{
    private readonly IConfiguration _configuration;

    public DbConnectionProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string GetConnectionString()
    {
        var catalog = new ConnectionCatalogBuilder()
            .Add(ConnectionNames.AdventureWorks, ConnectionKind.Sql)
            .Build(_configuration);

        var connection = catalog.Get(ConnectionNames.AdventureWorks, ConnectionKind.Sql);

        ConnectionValidation.EnsureNonEmpty(connection);

        return connection.Value;
    }

    public static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
