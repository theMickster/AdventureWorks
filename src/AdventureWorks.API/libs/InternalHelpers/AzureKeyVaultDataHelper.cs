using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Constants;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs.InternalHelpers;

internal static class AzureKeyVaultDataHelper
{
    internal static SecretClient GetAzureKeyVaultClient(IConfiguration configuration)
    {
        var akvUriValue = configuration[ConfigurationConstants.KeyVaultUrl] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(akvUriValue))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.KeyVaultUrl} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        var akvUri = new Uri(akvUriValue);

        var tenantId = configuration[ConfigurationConstants.KeyVaultTenantId] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.KeyVaultTenantId} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        var clientId = configuration[ConfigurationConstants.KeyVaultClientId] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.KeyVaultClientId} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        var clientSecret = configuration[ConfigurationConstants.KeyVaultClientSecret] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ConfigurationException(
                $"The required Configuration value for {ConfigurationConstants.KeyVaultClientSecret} is missing." +
                "Please verify local or Azure resource configuration.");
        }

        var akvClientSecretCredential = new ClientSecretCredential(
            tenantId: tenantId,
            clientId: clientId,
            clientSecret: clientSecret);

        if (!double.TryParse(configuration[ConfigurationConstants.KeyVaultRetryDelay], out var retryDelay))
        {
            retryDelay = 100d;
        }

        if (!double.TryParse(configuration[ConfigurationConstants.KeyVaultMaxRetryDelay], out var maxRetryDelay))
        {
            maxRetryDelay = 3000d;
        }

        if (!int.TryParse(configuration[ConfigurationConstants.KeyVaultMaxRetryAttempts], out var maxRetryCount))
        {
            maxRetryCount = 5;
        }

        var akvSecretOptions = new SecretClientOptions
        {
            Retry =
            {
                Mode = Azure.Core.RetryMode.Exponential,
                Delay = TimeSpan.FromMilliseconds(retryDelay),
                MaxDelay = TimeSpan.FromMilliseconds(maxRetryDelay),
                MaxRetries = maxRetryCount
            }
        };

        return new SecretClient(akvUri, akvClientSecretCredential, akvSecretOptions);
    }
}