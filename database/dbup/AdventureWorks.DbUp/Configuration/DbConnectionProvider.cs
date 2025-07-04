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
        var connectionString = _configuration.GetConnectionString("AdventureWorks");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'AdventureWorks' not found in configuration.");
        }

        return connectionString;
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
