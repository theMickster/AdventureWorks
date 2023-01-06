using Azure;
using Azure.Security.KeyVault.Secrets;
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Common.Helpers.Fakes;

[ExcludeFromCodeCoverage]
internal class FakeSecretClient : SecretClient
{
    private readonly IDictionary<string, string> values;

    public FakeSecretClient(IDictionary<string, string> values) : base() => this.values = values;

    public override Response<KeyVaultSecret> GetSecret(string name, string version = null, CancellationToken cancellationToken = default)
    {

        if (values.TryGetValue(name, out var val))
        {
            return new FakeResponse<KeyVaultSecret>(new KeyVaultSecret(name, val));
        }
        return new FakeResponse<KeyVaultSecret>(null);
    }

    public override Task<Response<KeyVaultSecret>> GetSecretAsync(string name, string version = null, CancellationToken cancellationToken = default)
    {
        if (values.TryGetValue(name, out var val))
        {
            return Task.FromResult((Response<KeyVaultSecret>)new FakeResponse<KeyVaultSecret>(new KeyVaultSecret(name, val)));
        }
        return Task.FromResult((Response<KeyVaultSecret>)new FakeResponse<KeyVaultSecret>(null));
    }
}
