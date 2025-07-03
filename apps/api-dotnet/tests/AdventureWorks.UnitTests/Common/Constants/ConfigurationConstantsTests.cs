using AdventureWorks.Common.Constants;

namespace AdventureWorks.UnitTests.Common.Constants;

[ExcludeFromCodeCoverage]
public sealed class ConfigurationConstantsTests
{
    [Fact]
    public void Type_has_correct_members()
    {
        using (new AssertionScope())
        {
            ConfigurationConstants.KeyVaultUrl.Should().Be("KeyVault:VaultUri");
            ConfigurationConstants.KeyVaultRetryDelay.Should().Be("KeyVault:RetryDelayMilliseconds");
            ConfigurationConstants.KeyVaultMaxRetryDelay.Should().Be("KeyVault:MaxRetryDelayMilliseconds");
            ConfigurationConstants.KeyVaultMaxRetryAttempts.Should().Be("KeyVault:MaxRetryAttempts");
            ConfigurationConstants.AppInsightsConnectionString.Should().Be("ApplicationInsights:ConnectionString");
            ConfigurationConstants.ApplicationEnvironment01.Should().Be("ASPNETCORE_ENVIRONMENT");
            ConfigurationConstants.ApplicationEnvironment02.Should().Be("APPSETTING_ASPNETCORE_ENVIRONMENT");
            ConfigurationConstants.SqlConnectionDefaultConnectionName.Should().Be("DefaultConnection");
            ConfigurationConstants.SqlConnectionSqlAzureConnectionName.Should().Be("SqlAzureConnection");
            ConfigurationConstants.CurrentConnectionStringNameKey.Should().Be("CurrentConnectionStringName");
        }
    }
}