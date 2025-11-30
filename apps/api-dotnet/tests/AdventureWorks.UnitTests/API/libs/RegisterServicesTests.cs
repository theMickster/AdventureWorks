using AdventureWorks.API.libs;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Settings;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AdventureWorks.UnitTests.API.libs;

[ExcludeFromCodeCoverage]
public sealed class RegisterServicesTests : UnitTestBase
{
    [Fact]
    public void GetDatabaseConnectionStrings_when_only_load_testing_connection_exists_succeeds()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ConfigurationConstants.SqlConnectionLoadTestingConnectionName}"] =
                    "Server=(local);Database=AdventureWorks_Load;Application Name=AdventureWorks.UnitTests;"
            })
            .Build();

        var result = InvokeGetDatabaseConnectionStrings(configuration);

        result.Should().ContainSingle();
        result[0].ConnectionStringName.Should().Be(ConfigurationConstants.SqlConnectionLoadTestingConnectionName);
    }

    [Fact]
    public void GetSqlConnectionString_when_load_testing_name_selected_returns_load_testing_connection()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{EntityFrameworkCoreSettings.SettingsRootName}:{ConfigurationConstants.CurrentConnectionStringNameKey}"] =
                ConfigurationConstants.SqlConnectionLoadTestingConnectionName
        });

        var connectionStrings = new List<DatabaseConnectionString>
        {
            new()
            {
                ConnectionStringName = ConfigurationConstants.SqlConnectionDefaultConnectionName,
                ConnectionString = "Server=(local);Database=AdventureWorks_Default;Application Name=AdventureWorks.UnitTests;"
            },
            new()
            {
                ConnectionStringName = ConfigurationConstants.SqlConnectionLoadTestingConnectionName,
                ConnectionString = "Server=(local);Database=AdventureWorks_Load;Application Name=AdventureWorks.UnitTests;"
            }
        };

        var result = InvokeGetSqlConnectionString(configuration, connectionStrings);

        result.Should().Contain("AdventureWorks_Load");
    }

    [Fact]
    public void GetSqlConnectionString_when_playwright_testing_name_selected_returns_playwright_testing_connection()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{EntityFrameworkCoreSettings.SettingsRootName}:{ConfigurationConstants.CurrentConnectionStringNameKey}"] =
                ConfigurationConstants.SqlConnectionPlaywrightTestingConnectionName
        });

        var connectionStrings = new List<DatabaseConnectionString>
        {
            new()
            {
                ConnectionStringName = ConfigurationConstants.SqlConnectionDefaultConnectionName,
                ConnectionString = "Server=(local);Database=AdventureWorks_Default;Application Name=AdventureWorks.UnitTests;"
            },
            new()
            {
                ConnectionStringName = ConfigurationConstants.SqlConnectionPlaywrightTestingConnectionName,
                ConnectionString = "Server=(local);Database=AdventureWorks_Playwright;Application Name=AdventureWorks.UnitTests;"
            }
        };

        var result = InvokeGetSqlConnectionString(configuration, connectionStrings);

        result.Should().Contain("AdventureWorks_Playwright");
    }

    private static List<DatabaseConnectionString> InvokeGetDatabaseConnectionStrings(IConfiguration configuration)
    {
        var method = typeof(RegisterServices).GetMethod("GetDatabaseConnectionStrings",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull();
        var result = method!.Invoke(null, [configuration]);

        result.Should().NotBeNull();
        return result!.As<List<DatabaseConnectionString>>();
    }

    private static string InvokeGetSqlConnectionString(ConfigurationManager configuration,
        IEnumerable<DatabaseConnectionString> connectionStrings)
    {
        var method = typeof(RegisterServices).GetMethod("GetSqlConnectionString",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull();
        var result = method!.Invoke(null, [configuration, connectionStrings]);

        result.Should().NotBeNull();
        return result!.As<string>();
    }
}
