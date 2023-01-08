using AdventureWorks.Common.Helpers.Fakes;
using Azure;
using Azure.Security.KeyVault.Secrets;
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Common.Helpers;

public static class SecretHelper
{
    public static SecretClient SecretClient { get; set; } = default!;

    /// <summary>
    /// Retrieves a secret by name
    /// </summary>
    /// <param name="secretName"></param>
    /// <returns></returns>
    public static string GetSecret(string secretName)
    {
        if (SecretClient == null)
        {
            throw new InvalidOperationException(
                "The secret client is not configured.  " +
                "This should not be called until after the call to LoadAuthenticatorConfiguration is completed");
        }

        try
        {
            var result = SecretClient.GetSecret(secretName);

            return result?.Value?.Value!;
        }
        catch (RequestFailedException)
        {
            return null!;
        }
    }

    /// <summary>
    /// Asynchronously retrieves a secret by name
    /// </summary>
    /// <param name="secretName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (SecretClient == null)
        {
            throw new InvalidOperationException(
                "The secret client is not configured.  " +
                "This should not be called until after the call to LoadAuthenticatorConfiguration is completed");
        }

        try
        {
            var result = await SecretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);

            return result?.Value?.Value!;
        }
        catch (RequestFailedException)
        {
            return null!;
        }
    }

    /// <summary>
    /// Use Mock/Fake azure key vault data structures.
    /// </summary>
    /// <param name="values">Dictionary</param>
    /// <remarks>
    /// This SHOULD ONLY for unit test purposes.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static void UseMockData(IDictionary<string, string> values)
    {
        if (SecretClient != null && SecretClient.GetType() != typeof(FakeSecretClient))
        {
            throw new InvalidOperationException(
                "The secret client is already set... " +
                "This method should only be run from unit tests and can only be run once!");
        }
        SecretClient = new FakeSecretClient(values);
    }
}