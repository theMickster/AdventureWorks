using AdventureWorks.API.libs.InternalHelpers;
using AdventureWorks.Application.Exceptions;
using AdventureWorks.Common.Helpers;

[assembly: InternalsVisibleTo("AdventureWorks.Test.UnitTests")]
namespace AdventureWorks.API.libs;

internal static class LoadAdventureWorksConfiguration
{
    internal static void LoadApplicationConfiguration(this IConfiguration configuration)
    {
        var akvClient = AzureKeyVaultDataHelper.GetAzureKeyVaultClient(configuration);

        if (akvClient == null)
        {
            throw new ConfigurationException(
                "Unable to create an Azure Key Vault secret helper. " +
                "A properly configured helper is required. " +
                "Please verify local or Azure resource configuration. ");
        }

        SecretHelper.SecretClient = akvClient;
    }
}