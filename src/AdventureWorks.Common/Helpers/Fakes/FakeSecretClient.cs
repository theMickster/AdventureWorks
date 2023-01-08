﻿using Azure;
using Azure.Security.KeyVault.Secrets;
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Common.Helpers.Fakes;

[ExcludeFromCodeCoverage]
internal class FakeSecretClient : SecretClient
{
    private readonly IDictionary<string, string> _values;

    public FakeSecretClient(IDictionary<string, string> values) : base() => this._values = values;

    public override Response<KeyVaultSecret> GetSecret(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        return _values.TryGetValue(name, out var val) ? 
            new FakeResponse<KeyVaultSecret>(new KeyVaultSecret(name, val)) : 
            new FakeResponse<KeyVaultSecret>(null!);
    }

    public override Task<Response<KeyVaultSecret>> GetSecretAsync(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        return _values.TryGetValue(name, out var val) ? 
            Task.FromResult((Response<KeyVaultSecret>)new FakeResponse<KeyVaultSecret>(new KeyVaultSecret(name, val))) : 
            Task.FromResult((Response<KeyVaultSecret>)new FakeResponse<KeyVaultSecret>(null!));
    }
}
