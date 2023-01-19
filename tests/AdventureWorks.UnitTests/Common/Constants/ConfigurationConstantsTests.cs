using AdventureWorks.Common.Constants;

namespace AdventureWorks.UnitTests.Common.Constants;

[ExcludeFromCodeCoverage]
public sealed class ConfigurationConstantsTests
{
    [Fact]
    public void Type_has_correct_members()
    {
        var sut = typeof(ConfigurationConstants);

        using (new AssertionScope())
        {
            ConfigurationConstants.KeyVaultClientId.Should().Be("KeyVault:ClientId");
            ConfigurationConstants.KeyVaultClientSecret.Should().Be("KeyVault:ClientSecret");
            ConfigurationConstants.KeyVaultTenantId.Should().Be("KeyVault:TenantId");
            ConfigurationConstants.KeyVaultUrl.Should().Be("KeyVault:VaultUri");
            ConfigurationConstants.KeyVaultRetryDelay.Should().Be("KeyVault:RetryDelayMilliseconds");
            ConfigurationConstants.KeyVaultMaxRetryDelay.Should().Be("KeyVault:MaxRetryDelayMilliseconds");
            ConfigurationConstants.KeyVaultMaxRetryAttempts.Should().Be("KeyVault:MaxRetryAttempts");
            ConfigurationConstants.AppInsightsInstrumentationKey.Should().Be("ApplicationInsights:InstrumentationKey");
            ConfigurationConstants.AppInsightsConnectionString.Should().Be("ApplicationInsights:ConnectionString");
            ConfigurationConstants.ApplicationEnvironment01.Should().Be("ASPNETCORE_ENVIRONMENT");
            ConfigurationConstants.ApplicationEnvironment02.Should().Be("APPSETTING_ASPNETCORE_ENVIRONMENT");
            ConfigurationConstants.SqlConnectionDefaultConnectionName.Should().Be("DefaultConnection");
            ConfigurationConstants.SqlConnectionSqlAzureConnectionName.Should().Be("SqlAzureConnection");
            ConfigurationConstants.CurrentConnectionStringNameKey.Should().Be("CurrentConnectionStringName");
        }
    }
}