using AdventureWorks.API.libs;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Settings;
using AdventureWorks.Connections;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AdventureWorks.UnitTests.API.libs;

[ExcludeFromCodeCoverage]
public sealed class RegisterServicesTests : UnitTestBase
{
    [Fact]
    public void GetActiveConnectionString_when_load_testing_name_selected_returns_load_testing_connection()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{EntityFrameworkCoreSettings.SettingsRootName}:{ConfigurationConstants.CurrentConnectionStringNameKey}"] =
                ConnectionNames.AdventureWorks_Load,
            [$"ConnectionStrings:{ConnectionNames.AdventureWorks}"] =
                "Server=(local);Database=AdventureWorks_Default;Application Name=AdventureWorks.UnitTests;",
            [$"ConnectionStrings:{ConnectionNames.AdventureWorks_Load}"] =
                "Server=(local);Database=AdventureWorks_Load;Application Name=AdventureWorks.UnitTests;"
        });

        var result = InvokeGetActiveConnectionString(configuration);

        result.Should().Contain("AdventureWorks_Load");
    }

    [Fact]
    public void GetActiveConnectionString_when_playwright_testing_name_selected_returns_playwright_testing_connection()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{EntityFrameworkCoreSettings.SettingsRootName}:{ConfigurationConstants.CurrentConnectionStringNameKey}"] =
                ConnectionNames.AdventureWorks_E2E,
            [$"ConnectionStrings:{ConnectionNames.AdventureWorks}"] =
                "Server=(local);Database=AdventureWorks_Default;Application Name=AdventureWorks.UnitTests;",
            [$"ConnectionStrings:{ConnectionNames.AdventureWorks_E2E}"] =
                "Server=(local);Database=AdventureWorks_Playwright;Application Name=AdventureWorks.UnitTests;"
        });

        var result = InvokeGetActiveConnectionString(configuration);

        result.Should().Contain("AdventureWorks_Playwright");
    }

    private static string InvokeGetActiveConnectionString(IConfiguration configuration)
    {
        var method = typeof(RegisterServices).GetMethod("GetActiveConnectionString",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull();
        var result = method!.Invoke(null, [configuration]);

        result.Should().NotBeNull();
        return result!.As<string>();
    }
}
