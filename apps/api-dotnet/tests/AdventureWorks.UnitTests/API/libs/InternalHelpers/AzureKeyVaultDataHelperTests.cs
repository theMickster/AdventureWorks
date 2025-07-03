using AdventureWorks.API.libs.InternalHelpers;
using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Constants;
using Microsoft.Extensions.Configuration;

namespace AdventureWorks.UnitTests.API.libs.InternalHelpers;

[ExcludeFromCodeCoverage]
public sealed class AzureKeyVaultDataHelperTests : UnitTestBase
{

    [Fact]
    public void GetAzureKeyVaultClient_when_all_settings_exist_and_are_correct_succeeds()
    {
        var settings = new Dictionary<string, string>
        {
            {"Key1", "Value1"},
            {"KeyVault:VaultUri", "https://akv-mick.azure.microsoft.com"},
            {"KeyVault:TenantId", new Guid().ToString()},
            {"KeyVault:ClientId", "HelloWorld"},
            {"KeyVault:ClientSecret", "HelloWorld2"},
            {"KeyVault:RetryDelayMilliseconds", "10"},
            {"KeyVault:MaxRetryDelayMilliseconds", "5"},
            {"KeyVault:MaxRetryAttempts", "12"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        var result = AzureKeyVaultDataHelper.GetAzureKeyVaultClient(configuration);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.VaultUri.Should().Be("https://akv-mick.azure.microsoft.com");
        }
    }


    [Fact]
    public void GetAzureKeyVaultClient_when_required_setting_exists_defaults_are_correct_succeeds()
    {
        var settings = new Dictionary<string, string>
        {
            {"Key1", "Value1"},
            {"KeyVault:VaultUri", "https://akv-mick.azure.microsoft.com"},
            {"KeyVault:TenantId", new Guid().ToString()},
            {"KeyVault:ClientId", "HelloWorld"},
            {"KeyVault:ClientSecret", "HelloWorld2"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        var result = AzureKeyVaultDataHelper.GetAzureKeyVaultClient(configuration);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.VaultUri.Should().Be("https://akv-mick.azure.microsoft.com");
        }
    }

    [Fact]
    public void GetAzureKeyVaultClient_when_key_vault_url_null_throws_correct_exception()
    {
        var settings = new Dictionary<string, string>
        {
            {"Key1", "Value1"},
            {"Nested:Key1", "NestedValue1"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        _ = ((Action)(() => _ = AzureKeyVaultDataHelper.GetAzureKeyVaultClient(configuration)))
            .Should().Throw<ConfigurationException>("because we expect a custom configuration exception.")
            .And.Message.Should().Contain(ConfigurationConstants.KeyVaultUrl);
    }
    
}